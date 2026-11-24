using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class TipoSenaService(
    ICurrentActor currentActor,
    IAdministrationStore administration,
    ITipoSenaStore store)
{
    public async Task<PageResult<TipoSenaRow>> GetPageAsync(TipoSenaQuery query)
    {
        await AuthorizeAsync(false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(TipoSenaInput input)
    {
        var actor = await AuthorizeAsync(true);
        var id = input.Id;

        TipoSena tipoSena;
        try
        {
            tipoSena = new TipoSena(
                id: id ?? Guid.NewGuid(),
                nombre: input.Nombre);
        }
        catch (ArgumentException exception)
        {
            throw new AdministrationException(exception.Message);
        }

        if (!id.HasValue)
        {
            await store.CreateAsync(tipoSena, actor.UserId);
            return;
        }

        await store.UpdateAsync(tipoSena, actor.UserId);
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
