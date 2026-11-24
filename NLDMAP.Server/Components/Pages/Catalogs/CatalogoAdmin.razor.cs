using Microsoft.AspNetCore.Components;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Server.Components.Administration;

namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class CatalogoAdmin : AdminPageBase
{
    [Parameter] public string Ruta { get; set; } = "";
    [Inject] private IServiceProvider Services { get; set; } = default!;

    private CatalogoVista? catalogo;
    private PageResult<FilaCatalogo> result = new([], 0, 1, 20);
    private Dictionary<string, string>? editing;
    private Guid? editingId;
    private string search = "";
    private int page = 1;
    private bool CanManage => Access?.Allows(PermissionNames.CatalogsManage) == true;

    protected override async Task OnParametersSetAsync()
    {
        var selected = CatalogoViews.Buscar(Ruta);
        if (selected == catalogo)
            return;

        catalogo = selected;
        CancelEdit();
        search = "";
        page = 1;
        result = new([], 0, 1, 20);
        await RunAsync(LoadAsync);
    }

    protected override async Task LoadAsync()
    {
        if (catalogo is not { } selected)
            return;

        var items = await selected.GetPageAsync(Services, search, page);
        if (selected != catalogo)
            return;

        result = items;
        page = result.Page;
    }

    private void NewOption()
    {
        if (catalogo is null)
            return;

        editingId = null;
        editing = catalogo.Campos.ToDictionary(campo => campo.Clave, _ => "");
    }

    private void Edit(FilaCatalogo row)
    {
        editingId = row.Id;
        editing = new Dictionary<string, string>(row.Valores);
    }

    private void CancelEdit()
    {
        editing = null;
        editingId = null;
    }

    private Task SaveAsync() => RunAsync(async () =>
    {
        if (catalogo is not { } selected || editing is not { } values)
            return;

        await selected.SaveAsync(Services, editingId, values);
        if (selected != catalogo)
            return;

        CancelEdit();
        SetNotice("Opción guardada.");
        await LoadAsync();
    });

    private Task DeleteAsync(Guid id) => RunAsync(async () =>
    {
        if (catalogo is not { } selected)
            return;

        await selected.DeleteAsync(Services, id);
        await LoadAsync();
    }, "Opción eliminada.");

    private Task SearchAsync() => ChangePageAsync(1);

    private Task ChangePageAsync(int selected) => RunAsync(async () =>
    {
        page = selected;
        await LoadAsync();
    });

    private static bool IsHex(string value) =>
        value.Length == 7 && value[0] == '#' && value.Skip(1).All(Uri.IsHexDigit);
}
