using Microsoft.AspNetCore.Components;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Application.Security;
using NLDMAP.Server.Components.Administration;

namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class RegionCorporalCatalog : AdminPageBase
{
    [Inject]
    private RegionCorporalService Catalog { get; set; } = default!;

    private PageResult<RegionCorporalRow> result = new([], 0, 1, 20);
    private RegionCorporalInput? editing;
    private string search = "";
    private int page = 1;

    private bool CanManage => Access?.Allows(PermissionNames.CatalogsManage) == true;

    protected override async Task LoadAsync()
    {
        result = await Catalog.GetPageAsync(
            new RegionCorporalQuery(Search: search, Page: page));

        page = result.Page;
    }

    private void NewOption() => editing = new RegionCorporalInput();

    private void Edit(RegionCorporalRow row)
    {
        editing = new RegionCorporalInput
        {
            Id = row.Id,
            Region = row.Region,
            Orden = row.Orden,
            Version = row.Version
        };
    }

    private void CancelEdit() => editing = null;

    private Task SaveAsync() => RunAsync(async () =>
    {
        if (editing is null)
            return;

        await Catalog.SaveAsync(editing);
        editing = null;
        await LoadAsync();
    }, "Región guardada.");

    private Task SearchAsync() => RunAsync(async () =>
    {
        page = 1;
        await LoadAsync();
    });

    private Task ChangePageAsync(int selected) => RunAsync(async () =>
    {
        page = selected;
        await LoadAsync();
    });
}