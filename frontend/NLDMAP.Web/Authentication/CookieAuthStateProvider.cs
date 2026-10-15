using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NLDMAP.Web.Administration;
using NLDMAP.Web.Administration.Caching;

namespace NLDMAP.Web.Authentication;

public sealed class CookieAuthStateProvider(
    AdminApiClient api, AdminCacheSession session, IJSRuntime javascript)
    : AuthenticationStateProvider, IAsyncDisposable
{
    private static AuthenticationState Anonymous => new(new ClaimsPrincipal(new ClaimsIdentity()));
    private Task<AuthenticationState>? stateTask;
    private Task? bridgeTask;
    private Task? refreshTask;
    private IJSObjectReference? bridge;
    private DotNetObjectReference<CookieAuthStateProvider>? reference;
    private string? knownRevision;
    private bool disposed;

    public string? ErrorMessage { get; private set; }
    public string? CoordinationWarning { get; private set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (disposed) { return Anonymous; }
        await EnsureBridgeAsync();
        if (bridge is not null)
        {
            try
            {
                var revision = await bridge.InvokeAsync<string>("getRevision");
                if (revision != knownRevision) { BeginRevision(revision, notify: true); }
            }
            catch (JSException) { DisableReuse(); }
        }
        // Without cross-tab coordination, prefer a fresh session to private cache reuse.
        if (!session.ReuseAllowed && stateTask?.IsCompleted == true)
        {
            BeginRevision(knownRevision, notify: false);
        }
        var pending = stateTask ??= LoadStateAsync(session.Generation);
        var state = await pending;
        return ReferenceEquals(stateTask, pending) ? state : await GetAuthenticationStateAsync();
    }

    // Called after login/logout and by the existing "Actualizar acceso" control.
    public Task RefreshAsync()
    {
        if (refreshTask is null || refreshTask.IsCompleted) { refreshTask = RefreshCoreAsync(); }
        return refreshTask;
    }

    private async Task RefreshCoreAsync()
    {
        await EnsureBridgeAsync();
        var revision = knownRevision;
        if (bridge is not null)
        {
            try { revision = await bridge.InvokeAsync<string>("publish"); }
            catch (JSException) { DisableReuse(); }
        }
        BeginRevision(revision, notify: true);
        var state = await GetAuthenticationStateAsync();
        if (!disposed) { NotifyAuthenticationStateChanged(Task.FromResult(state)); }
    }

    public void UpdateDisplayName(string userId, string firstName, string lastName)
    {
        if (stateTask?.IsCompletedSuccessfully != true) { return; }
        var current = stateTask.Result;
        if (current.User.FindFirst(ClaimTypes.NameIdentifier)?.Value != userId) { return; }
        var name = (firstName + " " + lastName).Trim();
        var claims = current.User.Claims.Where(claim => claim.Type != ClaimTypes.Name)
            .Append(new Claim(ClaimTypes.Name, name));
        var updated = new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Identity.Application")));
        stateTask = Task.FromResult(updated);
        NotifyAuthenticationStateChanged(stateTask);
    }

    [JSInvokable]
    public async Task OnBrowserSessionChanged()
    {
        // Read the latest revision instead of trusting the order of queued events.
        var state = await GetAuthenticationStateAsync();
        if (!disposed) { NotifyAuthenticationStateChanged(Task.FromResult(state)); }
    }

    private void BeginRevision(string? revision, bool notify)
    {
        knownRevision = revision;
        stateTask = null;
        session.Reset();
        if (notify && !disposed)
        {
            // Hide snapshots while a possibly different account is resolved.
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }
    }

    private Task EnsureBridgeAsync() => bridgeTask ??= StartBridgeAsync();

    private async Task StartBridgeAsync()
    {
        try
        {
            bridge = await javascript.InvokeAsync<IJSObjectReference>(
                "import", "./js/adminSessionSync.js");
            reference = DotNetObjectReference.Create(this);
            var supported = await bridge.InvokeAsync<bool>("start", reference);
            knownRevision = await bridge.InvokeAsync<string>("getRevision");
            if (!supported) { DisableReuse(); }
        }
        catch (JSException) { DisableReuse(); }
    }

    private void DisableReuse()
    {
        session.ReuseAllowed = false;
        CoordinationWarning = "Esta pestaña no puede sincronizar cambios de sesión. Los datos se volverán a consultar al abrir cada pantalla.";
    }

    private async Task<AuthenticationState> LoadStateAsync(long generation)
    {
        string? errorMessage = null;
        var state = Anonymous;
        string? ownerId = null;
        try
        {
            var snapshot = await api.GetSessionAsync(session.Token);
            if (string.IsNullOrWhiteSpace(snapshot.Id))
            {
                throw new ApiException("La sesión no contiene un identificador de cuenta válido.");
            }
            var displayName = (snapshot.FirstName + " " + snapshot.LastName).Trim();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, snapshot.Id),
                new(ClaimTypes.Name, string.IsNullOrEmpty(displayName) ? snapshot.Email : displayName),
                new(ClaimTypes.Email, snapshot.Email)
            };
            claims.AddRange((snapshot.Roles ?? []).Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange((snapshot.Permissions ?? [])
                .Select(permission => new Claim(AdminPermissions.ClaimType, permission)));
            state = new(new ClaimsPrincipal(new ClaimsIdentity(claims, "Identity.Application")));
            ownerId = snapshot.Id;
        }
        catch (ApiException error)
        {
            if (error.StatusCode != HttpStatusCode.Unauthorized) { errorMessage = error.Message; }
        }
        catch (OperationCanceledException) when (generation != session.Generation || disposed) { }
        if (generation == session.Generation && !disposed)
        {
            session.Establish(ownerId);
            ErrorMessage = errorMessage;
        }
        return state;
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (bridge is not null)
        {
            try
            {
                await bridge.InvokeVoidAsync("stop");
                await bridge.DisposeAsync();
            }
            catch (JSException) { }
        }
        reference?.Dispose();
    }
}
