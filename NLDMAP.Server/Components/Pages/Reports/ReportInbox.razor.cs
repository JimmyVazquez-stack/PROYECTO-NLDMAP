using System.Globalization;
using MudBlazor;
using NLDMAP.Server.Components.Reports.Models;
using NLDMAP.Server.Components.Reports.Preview;

namespace NLDMAP.Server.Components.Pages.Reports;

public partial class ReportInbox
{
    private readonly CultureInfo spanish = CultureInfo.GetCultureInfo("es-MX");
    private readonly MudTheme theme = new()
    {
        PaletteLight = new PaletteLight { Primary = "#173D3A", Background = "#F4F8F7", Surface = "#FFFFFF", TextPrimary = "#173D3A", TextSecondary = "#526D68" }
    };
    private IReadOnlyList<ReportInboxRow> rows = ReportInboxSampleData.Create();
    private string searchText = "";
    private string appliedSearch = "";
    private InboxStage activeStage = InboxStage.New;
    private bool ascending;
    private int page = 1;
    private int pageSize = 10;
    private ReportInboxRow? selected;
    private DateTimeOffset refreshedAt = DateTimeOffset.UtcNow;

    private IEnumerable<ReportInboxRow> MatchingRows => rows.Where(row =>
        spanish.CompareInfo.IndexOf(row.Folio, appliedSearch, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0 ||
        spanish.CompareInfo.IndexOf(row.PersonName, appliedSearch, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0);
    private int Count(InboxStage stage) => MatchingRows.Count(row => row.Stage == stage);
    private int Total => Count(activeStage);
    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Total / pageSize));
    private IEnumerable<ReportInboxRow> VisibleRows
    {
        get
        {
            var filtered = MatchingRows.Where(row => row.Stage == activeStage);
            var ordered = ascending ? filtered.OrderBy(row => row.RegisteredOn) : filtered.OrderByDescending(row => row.RegisteredOn);
            return ordered.ThenBy(row => row.Id).Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
    private static string StageName(InboxStage stage) => stage switch
    {
        InboxStage.New => "Recién añadidos", InboxStage.InProgress => "En proceso", _ => "Revisados"
    };
    private void Search() { appliedSearch = searchText.Trim(); ResetPage(); }
    private void SelectStage(InboxStage stage) { activeStage = stage; ResetPage(); }
    private void ResetPage() { page = 1; selected = null; }
    private void PreviousPage() { page = Math.Max(1, page - 1); selected = null; }
    private void NextPage() { page = Math.Min(TotalPages, page + 1); selected = null; }
    private void ClosePreview() => selected = null;
    private void Refresh()
    {
        rows = ReportInboxSampleData.Create();
        refreshedAt = DateTimeOffset.UtcNow;
        page = Math.Min(page, TotalPages);
        selected = null;
    }
}
