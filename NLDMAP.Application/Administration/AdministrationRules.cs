using NLDMAP.Application.Security;
namespace NLDMAP.Application.Administration;

public static class AdministrationRules
{
    public static void RequirePermission(AdminAccess access, string permission)
    {
        if (permission == PermissionNames.Access ? access.Permissions.Length == 0 : !access.Allows(permission))
            throw new AdministrationException("Tu sesión no permite realizar esta operación.");
    }
    public static void PreserveActiveAdministrator(int remainingBeforeRemoval)
    {
        if (remainingBeforeRemoval <= 1)
            throw new AdministrationException("Debe quedar al menos un administrador activo.");
    }
}
