using Microsoft.AspNetCore.Components;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Application.Security;
using NLDMAP.Server.Components.Administration;

namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class TezCatalog : AdminPageBase
{
    //Las primeras 4 variables provienen de Application.Catalogs
    [Inject] private TezService Catalog { get; set; } = default!;
    private PageResult<TezRow> result = new([], 0, 1, 5);
    private TezInput? editing;
    private string search = "";
    private int page = 1;
    //Esta variable nos da si tiene permiso para acceder a esa funcionalidad 'TezManage' declarada en Application
    private bool CanManage => Access?.Allows(PermissionNames.CatalogsManage) == true;

    protected override async Task LoadAsync()
    {
        Access = await Admin.GetAccessAsync();
        result = await Catalog.GetPageAsync(new TezQuery(Search: search, Page: page));
        page = result.Page;
    }

    private void NewOption()
    {
        editing = new TezInput();
    }

    private void Edit(TezRow row)
    {
        editing = new TezInput
        {
            Id = row.Id,
            ColorPiel = row.ColorPiel,
            ColorHex = row.ColorHex ?? "",
            Orden = row.Orden,
            Version = row.Version
        };
    }

    private void CancelEdit() => editing = null;

    private Task SaveAsync() => RunAsync(async () =>
    {
        if (editing is null) return;
        await Catalog.SaveAsync(editing);
        editing = null;
        await LoadAsync();
    }, "Opcion Guardada");
    
    private Task SearchAsync() => RunAsync(async () =>
    {
        page = 1;
        await LoadAsync();
    });

    private Task RefreshAsync() => RunAsync(async () =>
    {
        editing = null;
        await LoadAsync();
    });

    private Task ChangePageAsync(int selected) => RunAsync(async () =>
    {
        page = selected;
        await LoadAsync();
    });
    private static string? SwatchStyle(string? value)
    {
        var hex = value?.Trim();

        if (hex is null || hex.Length != 7 || hex[0] != '#' ||
            !hex.Skip(1).All(Uri.IsHexDigit))
        {
            return null;
        }

        return "display: inline-block; width: 1.25rem; height: 1.25rem; " +
               "border: 1px solid #999; border-radius: 50%; vertical-align: middle; " +
               $"background-color: {hex};";
    }
}
