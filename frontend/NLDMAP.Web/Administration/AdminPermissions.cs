namespace NLDMAP.Web.Administration;

public static class AdminPermissions
{
    public const string ClaimType = "permission";
    public const string UsersRead = "Users.Read";
    public const string UsersUpdate = "Users.Update";
    public const string UsersAssignRoles = "Users.AssignRoles";
    public const string RolesRead = "Roles.Read";
    public const string RolesUpdatePermissions = "Roles.UpdatePermissions";
    public const string SystemAdministratorRole = "Administrador del sistema";

    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[]
    {
        UsersRead, UsersUpdate, UsersAssignRoles, RolesRead, RolesUpdatePermissions
    });
}
