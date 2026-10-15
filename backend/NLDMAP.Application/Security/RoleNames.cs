namespace NLDMAP.Application.Security;

public static class RoleNames
{
    // Visitor describes public access and is not an assignable Identity role.
    public const string Visitor = "Visitante";

    public const string Reporter = "Reportante";
    public const string InformationContributor = "Aportante de información";
    public const string ReportManager = "Gestor de reportes";
    public const string Analyst = "Analista";
    public const string SystemAdministrator = "Administrador del sistema";

    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(
        new[]
        {
            Reporter,
            InformationContributor,
            ReportManager,
            Analyst,
            SystemAdministrator
        });

    public static bool IsAssignable(string roleName)
    {
        return All.Contains(roleName, StringComparer.Ordinal);
    }
}
