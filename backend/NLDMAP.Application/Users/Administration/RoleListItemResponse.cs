namespace NLDMAP.Application.Users.Administration;

public sealed record RoleListItemResponse(
    string Name,
    string[] Permissions,
    string[] AllowedPermissions,
    string Version);

