using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class TipoPrendaService(
    ICurrentActor currentActor,
    IAdministrationStore administration,
    ITipoPrendaStore store)
{
    public async Task<PageResult<TipoPrendaRow>> GetPageAsync(TipoPrendaQuery query)
    {
        await AuthorizeAsync(false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(TipoPrendaInput input)
    {
        var actor = await AuthorizeAsync(true);
        var id = input.Id;

        TipoVestimenta tipoPrenda;
        try
        {
            tipoPrenda = new TipoVestimenta(id ?? Guid.NewGuid(),
                CatalogInput.Required(input.Nombre, "Tipo de vestimenta", TipoVestimenta.NombreMaxLength));
        }

        catch (ArgumentException exception)
        {
            throw new AdministrationException(exception.Message);
        }

        if (!id.HasValue)
        {
            await store.CreateAsync(tipoPrenda, actor.UserId);
            return;
        }
        await store.UpdateAsync(tipoPrenda, actor.UserId);
    }

    public async Task DeleteAsync(Guid id)
    {
        await AuthorizeAsync(true);
        await store.DeleteAsync(id);
    }

    private async Task<ActorSession> AuthorizeAsync(bool write)
    {
        var actor = await currentActor.GetAsync();
        var access = await administration.GetAccessAsync(actor);

        AdministrationRules.RequirePermission(access, PermissionNames.CatalogsRead);
        if (write)
            AdministrationRules.RequirePermission(access, PermissionNames.CatalogsManage);
        return actor;
    }
}
