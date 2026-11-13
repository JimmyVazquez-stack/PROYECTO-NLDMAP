namespace NLDMAP.Application.Security;

public static class PermissionNames
{
    public const string ClaimType = "permission";
    public const string Access = "Administration.Access";
    public const string UsersRead = "Users.Read";
    public const string UsersEdit = "Users.Edit";
    public const string UsersStatus = "Users.Status";
    public const string RolesRead = "Roles.Read";
    public const string RolesManage = "Roles.Manage";
    public const string RolesAssign = "Roles.Assign";
    public const string AuditRead = "Audit.Read";
    public const string InvitationsManage = "Invitations.Manage";
    public const string CatalogsRead = "Catalogs.Read";
    public const string CatalogsManage = "Catalogs.Manage";

    public static readonly IReadOnlyDictionary<string, string> Catalog =
        new Dictionary<string, string>
        {
            [UsersRead] = "Consultar usuarios", [UsersEdit] = "Editar perfiles",
            [UsersStatus] = "Activar y desactivar cuentas", [RolesRead] = "Consultar roles",
            [RolesManage] = "Administrar roles y permisos", [RolesAssign] = "Asignar roles",
            [AuditRead] = "Consultar auditoría", [InvitationsManage] = "Administrar invitaciones",
            [CatalogsRead] = "Consultar catalogos",[CatalogsManage] = "Administrar catalogos",
        };
}
