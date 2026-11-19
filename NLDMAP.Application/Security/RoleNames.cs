namespace NLDMAP.Application.Security;

public static class RoleNames
{
    // Esta constante defiine el nombre del rol de administrador del sistema
    // Se utiliza para asignar permisos y privilegios especiales a los usuarios que pertenecen a este rol
    public const string Administrator = "Administrador del sistema";
    public const string User = "Usuario";
    public const string Guest = "Invitado";
    public const string Reporter = "Reportante";

    public static bool IsCitizenRole(string? name) =>
        string.Equals(name, Guest, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, Reporter, StringComparison.OrdinalIgnoreCase);

    public static bool IsCitizenAccount(ICollection<string> roles) =>
        roles.Count == 0 || roles.All(IsCitizenRole);

    public static readonly string[] SystemRoles =
        [Administrator, Guest, Reporter];

}