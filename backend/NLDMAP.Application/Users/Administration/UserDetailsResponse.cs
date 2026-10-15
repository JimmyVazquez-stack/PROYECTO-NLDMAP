namespace NLDMAP.Application.Users.Administration;

public sealed record UserDetailsResponse(
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
