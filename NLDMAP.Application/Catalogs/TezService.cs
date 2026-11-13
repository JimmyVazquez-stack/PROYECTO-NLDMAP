using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class TezService(ICurrentActor currentActor, IAdministrationStore administration,
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

        if (id.HasValue)
        {
            var current = await store.GetAsync(id.Value)
                          ?? throw new AdministrationException(
                              "La opción ya no existe. Actualiza el catálogo.");

            if (string.IsNullOrEmpty(input.Version) || input.Version != current.Version)
                throw new AdministrationException(
                    "Otra persona modificó esta opción. Actualiza y vuelve a editarla.");
        }

        Tez tez;
        try
        {
            tez = new Tez(
                id: id ?? Guid.NewGuid(),
                colorPiel: input.ColorPiel,
                colorHex: input.ColorHex,
                orden: input.Orden);
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

        await store.UpdateAsync(tez, input.Version, actor.UserId);
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
