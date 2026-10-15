namespace NLDMAP.Application.Security;

public static class PermissionNames
{
    public const string ClaimType = "permission";

    public const string UsersRead = "Users.Read";
    public const string UsersCreate = "Users.Create";
    public const string UsersUpdate = "Users.Update";
    public const string UsersAssignRoles = "Users.AssignRoles";
    public const string RolesRead = "Roles.Read";
    public const string RolesUpdatePermissions = "Roles.UpdatePermissions";
    public const string SettingsRead = "Settings.Read";
    public const string SettingsUpdate = "Settings.Update";

    public const string ReportsCreate = "Reports.Create";
    public const string ReportsReadOwn = "Reports.ReadOwn";
    public const string ReportsRespondToCorrections = "Reports.RespondToCorrections";
    public const string ReportsAddOwnUpdates = "Reports.AddOwnUpdates";

    public const string ContributionsCreate = "Contributions.Create";
    public const string ContributionsReadOwn = "Contributions.ReadOwn";

    public const string ReportsReadAssigned = "Reports.ReadAssigned";
    public const string ContributionsReadAssigned = "Contributions.ReadAssigned";
    public const string ReportsRequestCorrectionsAssigned = "Reports.RequestCorrectionsAssigned";
    public const string ContributionsRequestCorrectionsAssigned = "Contributions.RequestCorrectionsAssigned";
    public const string ReportsDecidePublicationAssigned = "Reports.DecidePublicationAssigned";
    public const string ContributionsDecidePublicationAssigned = "Contributions.DecidePublicationAssigned";
    public const string ReportsAddJustifiedUpdatesAssigned = "Reports.AddJustifiedUpdatesAssigned";

    public const string InformationReadAuthorized = "Information.ReadAuthorized";
    public const string AnalysisRecordObservations = "Analysis.RecordObservations";
    public const string StatisticsReadAuthorized = "Statistics.ReadAuthorized";

    public static IReadOnlyList<string> Technical { get; } = Array.AsReadOnly(
        new[]
        {
            UsersRead,
            UsersCreate,
            UsersUpdate,
            UsersAssignRoles,
            RolesRead,
            RolesUpdatePermissions,
            SettingsRead,
            SettingsUpdate
        });

    public static IReadOnlyList<string> Functional { get; } = Array.AsReadOnly(
        new[]
        {
            ReportsCreate,
            ReportsReadOwn,
            ReportsRespondToCorrections,
            ReportsAddOwnUpdates,
            ContributionsCreate,
            ContributionsReadOwn,
            ReportsReadAssigned,
            ContributionsReadAssigned,
            ReportsRequestCorrectionsAssigned,
            ContributionsRequestCorrectionsAssigned,
            ReportsDecidePublicationAssigned,
            ContributionsDecidePublicationAssigned,
            ReportsAddJustifiedUpdatesAssigned,
            InformationReadAuthorized,
            AnalysisRecordObservations,
            StatisticsReadAuthorized
        });

    public static IReadOnlyList<string> All { get; } =
        Array.AsReadOnly(Technical.Concat(Functional).ToArray());
}
