namespace NLDMAP.Application.Security;


public static class RoleNames
{
    // Esta constante defiine el nombre del rol de administrador del sistema
    // Se utiliza para asignar permisos y privilegios especiales a los usuarios que pertenecen a este rol
    public const string Administrator = "Administrador del sistema";
    public const string User = "Usuario";
    public const string Guest = "Invitado";
    public static readonly string[] SystemRoles =
        [Administrator];

}
