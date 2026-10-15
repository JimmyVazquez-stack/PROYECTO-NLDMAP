using System.Net;
using System.Security.Claims;
using NLDMAP.Web.Administration.Caching;
using NLDMAP.Web.Authentication;

namespace NLDMAP.Web.Administration;

public sealed class AdminDataStore : IDisposable
{
    private readonly AdminApiClient api;
    private readonly CookieAuthStateProvider auth;
    private readonly AdminCacheSession session;
    private readonly AdminDataChangeNotifier changes;
    private readonly BoundedQueryCache<UserQuery, AccountPage> users = new(30);
    private readonly BoundedQueryCache<string, RoleDefinition[]> roles = new(1);
    private readonly BoundedQueryCache<string, AccountDetails> details = new(100);
    private readonly BoundedQueryCache<HistoryQuery, ProfileHistory> history = new(20);
    private readonly BoundedQueryCache<HistoryQuery, AccountStatusHistory> statusHistory = new(20);
    private readonly Dictionary<string, DetailViewState> detailViews = new(StringComparer.Ordinal);

    public UserViewState UsersView { get; private set; } = new("", 1, 20);
    public string? SelectedRoleName { get; private set; }
    public long DataVersion => changes.Version;
    public string? SynchronizationWarning => changes.WarningMessage;
    public event Action? DataChanged;

    public AdminDataStore(AdminApiClient api, CookieAuthStateProvider auth,
        AdminCacheSession session, AdminDataChangeNotifier changes)
    {
        this.api = api;
        this.auth = auth;
        this.session = session;
        this.changes = changes;
        session.Resetting += Clear;
        changes.Changed += OnRemoteDataChanged;
    }

    public Task<CachedResponse<AccountPage>> GetUsersAsync(
        int pageNumber, int pageSize, string search, bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var key = new UserQuery((search ?? "").Trim(), pageNumber, pageSize);
        return ReadAsync(AdminPermissions.UsersRead, users, key,
            token => api.GetUsersAsync(pageNumber, pageSize, key.Search, token),
            forceRefresh, cancellationToken);
    }

    public Task<CachedResponse<RoleDefinition[]>> GetRolesAsync(
        bool forceRefresh = false, CancellationToken cancellationToken = default) =>
        ReadAsync(AdminPermissions.RolesRead, roles, "roles", api.GetRolesAsync,
            forceRefresh, cancellationToken);

    public Task<CachedResponse<AccountDetails>> GetUserDetailsAsync(
        string userId, bool forceRefresh = false, CancellationToken cancellationToken = default) =>
        ReadAsync(AdminPermissions.UsersRead, details, userId,
            token => api.GetUserDetailsAsync(userId, token), forceRefresh, cancellationToken);

    public Task<CachedResponse<ProfileHistory>> GetUserHistoryAsync(
        string userId, int pageNumber = 1, int pageSize = 20, bool forceRefresh = false,
        CancellationToken cancellationToken = default) =>
        ReadAsync(AdminPermissions.UsersRead, history, new HistoryQuery(userId, pageNumber, pageSize),
            token => api.GetUserHistoryAsync(userId, pageNumber, pageSize, token),
            forceRefresh, cancellationToken);

    private async Task<CachedResponse<TValue>> ReadAsync<TKey, TValue>(
        string permission, BoundedQueryCache<TKey, TValue> cache, TKey key,
        Func<CancellationToken, Task<TValue>> loader, bool forceRefresh,
        CancellationToken callerToken) where TKey : notnull
    {
        var scope = await GetScopeAsync(permission, callerToken);
        try
        {
            var result = await cache.GetAsync(key, loader,
                forceRefresh || !session.ReuseAllowed || !changes.IsAvailable, scope.Token, callerToken);
            EnsureCurrent(scope, callerToken);
            if (scope.DataVersion != changes.Version) { throw new CacheInvalidatedException(); }
            return result with { ChangeVersion = scope.DataVersion };
        }
        catch (ApiException error)
        {
            await ObserveAccessFailureAsync(scope, error);
            throw;
        }
        catch (OperationCanceledException) when (
            scope.Generation != session.Generation && !callerToken.IsCancellationRequested)
        {
            throw new CacheInvalidatedException();
        }
    }

    public Task<CachedResponse<AccountStatusHistory>> GetAccountStatusHistoryAsync(
        string userId, int pageNumber = 1, int pageSize = 20, bool forceRefresh = false,
        CancellationToken cancellationToken = default) =>
        ReadAsync(AdminPermissions.UsersRead, statusHistory, new HistoryQuery(userId, pageNumber, pageSize),
            token => api.GetAccountStatusHistoryAsync(userId, pageNumber, pageSize, token),
            forceRefresh, cancellationToken);

    public async Task<AccountDetails> UpdateAccountStatusAsync(
        AccountDetails account, bool isActive, string reason, CancellationToken cancellationToken = default)
    {
        var scope = await GetScopeAsync(AdminPermissions.UsersUpdate, cancellationToken);
        using var request = CancellationTokenSource.CreateLinkedTokenSource(scope.Token, cancellationToken);
        InvalidateAccount(account.Id, includeHistory: true);
        try
        {
            var saved = await api.UpdateAccountStatusAsync(account, isActive, reason, request.Token);
            EnsureCurrent(scope, cancellationToken);
            InvalidateAccount(account.Id, includeHistory: true);
            if (scope.DataVersion == changes.Version) { details.Put(saved.Id, saved); }
            if (saved.Version != account.Version) { await changes.PublishAsync(); }
            EnsureCurrent(scope, cancellationToken);
            return saved;
        }
        catch (ApiException error)
        {
            if (scope.Generation == session.Generation) { InvalidateAccount(account.Id, includeHistory: true); }
            await ObserveAccessFailureAsync(scope, error);
            throw;
        }
        catch
        {
            if (scope.Generation == session.Generation) { InvalidateAccount(account.Id, includeHistory: true); }
            throw;
        }
    }

    public async Task<AccountDetails> UpdateUserProfileAsync(
        AccountDetails account, string firstName, string lastName,
        CancellationToken cancellationToken = default)
    {
        var scope = await GetScopeAsync(AdminPermissions.UsersUpdate, cancellationToken);
        using var request = CancellationTokenSource.CreateLinkedTokenSource(scope.Token, cancellationToken);
        InvalidateAccount(account.Id, includeHistory: true);
        try
        {
            var saved = await api.UpdateUserProfileAsync(account, firstName, lastName, request.Token);
            EnsureCurrent(scope, cancellationToken);
            InvalidateAccount(account.Id, includeHistory: true);
            if (scope.DataVersion == changes.Version) { details.Put(saved.Id, saved); }
            auth.UpdateDisplayName(saved.Id, saved.FirstName, saved.LastName);
            if (saved.Version != account.Version) { await changes.PublishAsync(); }
            EnsureCurrent(scope, cancellationToken);
            return saved;
        }
        catch (ApiException error)
        {
            if (scope.Generation == session.Generation)
            {
                InvalidateAccount(account.Id, includeHistory: true);
            }
            await ObserveAccessFailureAsync(scope, error);
            throw;
        }
        catch
        {
            // A timeout can hide a committed write. Never keep an assumed old snapshot.
            if (scope.Generation == session.Generation)
            {
                InvalidateAccount(account.Id, includeHistory: true);
            }
            throw;
        }
    }

    public async Task ChangeRoleAsync(string userId, string roleName, bool remove,
        CancellationToken cancellationToken = default)
    {
        var scope = await GetScopeAsync(AdminPermissions.UsersAssignRoles, cancellationToken);
        using var request = CancellationTokenSource.CreateLinkedTokenSource(scope.Token, cancellationToken);
        InvalidateAccount(userId, includeHistory: false);
        try
        {
            await api.ChangeRoleAsync(userId, roleName, remove, request.Token);
            EnsureCurrent(scope, cancellationToken);
            await changes.PublishAsync();
            EnsureCurrent(scope, cancellationToken);
        }
        catch (ApiException error)
        {
            await ObserveAccessFailureAsync(scope, error);
            throw;
        }
        finally
        {
            if (scope.Generation == session.Generation)
            {
                InvalidateAccount(userId, includeHistory: false);
            }
        }
    }

    public async Task<RoleDefinition> UpdatePermissionsAsync(
        RoleDefinition role, IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        var scope = await GetScopeAsync(AdminPermissions.RolesUpdatePermissions, cancellationToken);
        using var request = CancellationTokenSource.CreateLinkedTokenSource(scope.Token, cancellationToken);
        var catalog = roles.Peek("roles")?.Value;
        roles.Clear();
        try
        {
            var saved = await api.UpdatePermissionsAsync(role, permissions, request.Token);
            EnsureCurrent(scope, cancellationToken);
            roles.Clear();
            if (catalog is not null && scope.DataVersion == changes.Version)
            {
                roles.Put("roles", catalog.Select(item => item.Name == saved.Name ? saved : item).ToArray());
            }
            if (saved.Version != role.Version) { await changes.PublishAsync(); }
            EnsureCurrent(scope, cancellationToken);
            if (scope.User.IsInRole(role.Name))
            {
                await auth.RefreshAsync();
            }
            return saved;
        }
        catch (ApiException error)
        {
            if (scope.Generation == session.Generation)
            {
                roles.Clear();
            }
            await ObserveAccessFailureAsync(scope, error);
            throw;
        }
        catch
        {
            if (scope.Generation == session.Generation) { roles.Clear(); }
            throw;
        }
    }

    public void RememberUsers(string search, int pageNumber, int pageSize) =>
        UsersView = new(search, pageNumber, pageSize);

    public void RememberRole(string? name) => SelectedRoleName = name;

    public DetailViewState GetDetailView(string userId) =>
        detailViews.GetValueOrDefault(userId) ?? new(false, 1);

    public void RememberDetail(string userId, bool showHistory, int historyPage)
    {
        if (!detailViews.ContainsKey(userId) && detailViews.Count >= 100)
        {
            detailViews.Remove(detailViews.Keys.First());
        }
        detailViews[userId] = GetDetailView(userId) with { ShowHistory = showHistory, HistoryPage = historyPage };
    }

    public void RememberStatusHistory(string userId, bool show, int pageNumber)
    {
        RememberDetail(userId, GetDetailView(userId).ShowHistory, GetDetailView(userId).HistoryPage);
        detailViews[userId] = GetDetailView(userId) with { ShowStatusHistory = show, StatusHistoryPage = pageNumber };
    }

    private void InvalidateAccount(string userId, bool includeHistory)
    {
        users.Clear();
        details.RemoveWhere(key => key == userId);
        if (includeHistory)
        {
            history.RemoveWhere(key => key.UserId == userId);
            statusHistory.RemoveWhere(key => key.UserId == userId);
        }
    }

    private async Task<SessionScope> GetScopeAsync(string permission, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var state = await auth.GetAuthenticationStateAsync();
        token.ThrowIfCancellationRequested();
        var generation = session.Generation;
        await changes.CheckForUpdatesAsync();
        token.ThrowIfCancellationRequested();
        if (generation != session.Generation) { throw new CacheInvalidatedException(); }
        var owner = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (state.User.Identity?.IsAuthenticated != true || owner != session.OwnerId)
        {
            throw new ApiException("Inicia sesión para continuar.", HttpStatusCode.Unauthorized);
        }
        if (!state.User.HasClaim(AdminPermissions.ClaimType, permission))
        {
            throw new ApiException("Tu cuenta no tiene permiso para esta operación.", HttpStatusCode.Forbidden);
        }
        return new(session.Generation, owner!, session.Token, state.User, changes.Version);
    }

    private async Task ObserveAccessFailureAsync(SessionScope scope, ApiException error)
    {
        if (scope.Generation != session.Generation) { throw new CacheInvalidatedException(); }
        if (error.IsAccessDenied) { await auth.RefreshAsync(); }
    }

    private void EnsureCurrent(SessionScope scope, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (scope.Generation != session.Generation || scope.OwnerId != session.OwnerId)
        {
            throw new CacheInvalidatedException();
        }
    }

    private void Clear()
    {
        ClearData();
        detailViews.Clear();
        UsersView = new("", 1, 20);
        SelectedRoleName = null;
    }

    private void ClearData()
    {
        users.Clear();
        roles.Clear();
        details.Clear();
        history.Clear();
        statusHistory.Clear();
    }

    private void OnRemoteDataChanged()
    {
        ClearData();
        DataChanged?.Invoke();
    }

    public void Dispose()
    {
        session.Resetting -= Clear;
        changes.Changed -= OnRemoteDataChanged;
    }

    private sealed record UserQuery(string Search, int PageNumber, int PageSize);
    private sealed record HistoryQuery(string UserId, int PageNumber, int PageSize);
    private sealed record SessionScope(
        long Generation, string OwnerId, CancellationToken Token, ClaimsPrincipal User, long DataVersion);
}

public sealed record UserViewState(string Search, int PageNumber, int PageSize);
public sealed record DetailViewState(
    bool ShowHistory, int HistoryPage, bool ShowStatusHistory = false, int StatusHistoryPage = 1);
