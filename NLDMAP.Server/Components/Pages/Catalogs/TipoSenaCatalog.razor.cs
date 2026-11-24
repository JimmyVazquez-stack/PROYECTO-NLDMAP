using Microsoft.AspNetCore.Components;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Application.Security;
using NLDMAP.Server.Components.Administration;

namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class TipoSenaCatalog : AdminPageBase
{
    [Inject]
    private TipoSenaService Catalog { get; set; } = default!;

    private PageResult<TipoSenaRow> result = new([], 0, 1, 20);
    private TipoSenaInput? editing;
    private string search = "";
    private int page = 1;

    private bool CanManage => Access?.Allows(PermissionNames.CatalogsManage) == true;

    protected override async Task LoadAsync()
    {
        result = await Catalog.GetPageAsync(
            new TipoSenaQuery(Search: search, Page: page));

        page = result.Page;
    }

    private void NewOption() => editing = new TipoSenaInput();

    private void Edit(TipoSenaRow row)
    {
        editing = new TipoSenaInput
        {
            Id = row.Id,
            Nombre = row.Nombre
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
    }, "Tipo de seña guardado.");

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

    private Task DeleteAsync(Guid id) => RunAsync(async () =>
    {
        await Catalog.DeleteAsync(id);
        await LoadAsync();
    }, "Opción eliminada.");
}
