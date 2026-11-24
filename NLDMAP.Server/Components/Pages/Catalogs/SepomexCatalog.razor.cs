using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;
using NLDMAP.Server.Components.Administration;

namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class SepomexCatalog : AdminPageBase
{
    [Inject] private SepomexService Catalog { get; set; } = default!;

    private PageResult<AsentamientoPostal> result = new([], 0, 1, 20);
    private string search = "";
    private string fileName = "";
    private int page = 1;
    private int uploadKey;
    private bool importing;
    private bool CanManage => Access?.Allows(PermissionNames.CatalogsManage) == true;

    protected override async Task LoadAsync()
    {
        result = await Catalog.GetPageAsync(search, page);
        page = result.Page;
    }

    private Task ImportAsync(InputFileChangeEventArgs args) => RunAsync(async () =>
    {
        try
        {
            if (args.FileCount != 1 ||
                !string.Equals(Path.GetExtension(args.File.Name), ".txt", StringComparison.OrdinalIgnoreCase))
                throw new AdministrationException("Selecciona un solo archivo TXT de SEPOMEX.");
            if (args.File.Size > SepomexService.MaxFileBytes)
                throw new AdministrationException("El TXT supera el tamaño máximo de 32 MB.");

            fileName = args.File.Name;
            importing = true;
            await using var stream = args.File.OpenReadStream(SepomexService.MaxFileBytes);
            var import = await Catalog.ImportAsync(stream);
            var unchanged = import.Procesados - import.Agregados - import.Actualizados;
            SetNotice($"{import.Procesados:N0} asentamientos procesados · {import.Agregados:N0} nuevos · " +
                $"{import.Actualizados:N0} actualizados · {unchanged:N0} sin cambios.");
            page = 1;
            search = "";
            await LoadAsync();
        }
        finally
        {
            importing = false;
            uploadKey++;
        }
    });

    private Task SearchAsync() => ChangePageAsync(1);

    private Task ChangePageAsync(int selected) => RunAsync(async () =>
    {
        page = selected;
        await LoadAsync();
    });
}
