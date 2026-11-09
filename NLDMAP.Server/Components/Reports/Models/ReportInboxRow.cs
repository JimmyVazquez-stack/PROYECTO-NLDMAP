namespace NLDMAP.Server.Components.Reports.Models;

// Presentation models only; these are not persistence entities.
public enum InboxStage { New, InProgress, Reviewed }

public sealed record ReportInboxRow(
    Guid Id, string Folio, string PersonName, DateOnly RegisteredOn,
    string ReviewStatus, string PublicationStatus, InboxStage Stage,
    string Description);
