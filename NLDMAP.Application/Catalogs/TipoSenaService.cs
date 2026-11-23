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

        if (id.HasValue)
        {
            var current = await store.GetAsync(id.Value)
                        ?? throw new AdministrationException(
                        "El tipo de seña ya no existe. Actualiza el catálogo.");

            if (string.IsNullOrEmpty(input.Version) || input.Version != current.Version)
                throw new AdministrationException(
                    "Otra persona modificó este tipo de seña. Actualiza el catálogo y vuelve a editarlo.");
        }

        TipoSena tipoSena;
        try
        {
            tipoSena = new TipoSena(
                id: id ?? Guid.NewGuid(),
                nombre: input.Nombre,
                orden: input.Orden);
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

        await store.UpdateAsync(tipoSena, input.Version, actor.UserId);
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
