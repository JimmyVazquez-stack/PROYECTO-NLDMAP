using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class TezService(
    ICurrentActor currentActor,
    IAdministrationStore administration,
    ITezStore store)
{
    public async Task<PageResult<TezRow>> GetPageAsync(TezQuery query)
    {
        await AuthorizeAsync(false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(TezInput input)
    {
        var actor = await AuthorizeAsync(true);
        var id = input.Id;

        Tez tez;
        try
        {
            tez = new Tez(
                id: id ?? Guid.NewGuid(),
                nombreColor: input.NombreColor);
        }
        catch (ArgumentException exception)
        {
            throw new AdministrationException(exception.Message);
        }

        if (!id.HasValue)
        {
            await store.CreateAsync(tez, actor.UserId);
            return;
        }

        await store.UpdateAsync(tez, actor.UserId);
    }

    public async Task DeleteAsync(Guid id)
    {
        await AuthorizeAsync(true);
        await store.DeleteAsync(id);
    }

    private async Task<ActorSession> AuthorizeAsync(bool write)
    {
        var actor = await currentActor.GetAsync();
        // Consulta la cuenta y sus permisos vigentes en cada operación del circuito Blazor.
        var access = await administration.GetAccessAsync(actor);
        AdministrationRules.RequirePermission(access, PermissionNames.CatalogsRead);
        if (write)
            AdministrationRules.RequirePermission(access, PermissionNames.CatalogsManage);
        return actor;
    }
}
