namespace NLDMAP.Web.Administration;

public sealed record AccountPage(
    AccountSummary[] Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record AccountSummary(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string[] Roles,
    string[] RetiredRoles);

public sealed record RoleDefinition(
    string Name,
    string[] Permissions,
    string[] AllowedPermissions,
    string Version);

public sealed record SessionSnapshot(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string[] Roles,
    string[] Permissions);

public sealed record CsrfToken(string HeaderName, string RequestToken);

public sealed record AccountDetails(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string[] Roles,
    string[] RetiredRoles,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    int AccessFailedCount,
    string Version,
    bool IsActive = true);

public sealed record AccountStatusChange(
    long Id, string UserId, string ActorUserId, string ActorDisplayName,
    DateTimeOffset OccurredAtUtc, bool PreviousIsActive, bool NewIsActive, string Reason);

public sealed record AccountStatusHistory(
    AccountStatusChange[] Items,
    int PageNumber, int PageSize, int TotalCount, int TotalPages);

public sealed record ProfileChange(
    long Id,
    string UserId,
    string ActorUserId,
    string ActorDisplayName,
    DateTimeOffset OccurredAtUtc,
    string PreviousFirstName,
    string PreviousLastName,
    string NewFirstName,
    string NewLastName);

public sealed record ProfileHistory(
    ProfileChange[] Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
