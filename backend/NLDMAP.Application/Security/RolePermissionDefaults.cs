namespace NLDMAP.Application.Security;

public static class RolePermissionDefaults
{
    private static readonly IReadOnlyList<string> ReporterPermissions =
        Array.AsReadOnly(new[]
        {
            PermissionNames.ReportsCreate,
            PermissionNames.ReportsReadOwn,
            PermissionNames.ReportsRespondToCorrections,
            PermissionNames.ReportsAddOwnUpdates
        });

    private static readonly IReadOnlyList<string> ContributorPermissions =
        Array.AsReadOnly(new[]
        {
            PermissionNames.ContributionsCreate,
            PermissionNames.ContributionsReadOwn
        });

    private static readonly IReadOnlyList<string> ManagerPermissions =
        Array.AsReadOnly(new[]
        {
            PermissionNames.ReportsReadAssigned,
            PermissionNames.ContributionsReadAssigned,
            PermissionNames.ReportsRequestCorrectionsAssigned,
            PermissionNames.ContributionsRequestCorrectionsAssigned,
            PermissionNames.ReportsDecidePublicationAssigned,
            PermissionNames.ContributionsDecidePublicationAssigned,
            PermissionNames.ReportsAddJustifiedUpdatesAssigned
        });

    private static readonly IReadOnlyList<string> AnalystPermissions =
        Array.AsReadOnly(new[]
        {
            PermissionNames.InformationReadAuthorized,
            PermissionNames.AnalysisRecordObservations,
            PermissionNames.StatisticsReadAuthorized
        });

    // These sets are also the maximum permission boundaries for the stable roles.
    public static IReadOnlyList<string> GetAllowedPermissions(string roleName)
    {
        return roleName switch
        {
            RoleNames.Reporter => ReporterPermissions,
            RoleNames.InformationContributor => ContributorPermissions,
            RoleNames.ReportManager => ManagerPermissions,
            RoleNames.Analyst => AnalystPermissions,
            RoleNames.SystemAdministrator => PermissionNames.Technical,
            _ => Array.Empty<string>()
        };
    }

    public static bool Allows(string roleName, string permission)
    {
        return GetAllowedPermissions(roleName)
            .Contains(permission, StringComparer.Ordinal);
    }

    public static string[] GetRolesForPermission(string permission)
    {
        return RoleNames.All
            .Where(role => Allows(role, permission))
            .ToArray();
    }
}
