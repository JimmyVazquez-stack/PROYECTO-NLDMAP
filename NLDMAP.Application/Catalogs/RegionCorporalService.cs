using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class RegionCorporalService(
    ICurrentActor currentActor,
    IAdministrationStore administration,
    IRegionCorporalStore store)
{
    public async Task<PageResult<RegionCorporalRow>> GetPageAsync(
        RegionCorporalQuery query)
    {
        await AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(RegionCorporalInput input)
    {
        var actor = await AuthorizeAsync(write: true);
        var id = input.Id;

        if (id.HasValue)
        {
            var current = await store.GetAsync(id.Value)
                ?? throw new AdministrationException(
                    "La región ya no existe. Actualiza el catálogo.");

            if (string.IsNullOrEmpty(input.Version) || input.Version != current.Version)
            {
                throw new AdministrationException(
                    "Otra persona modificó esta región. Actualiza el catálogo y vuelve a editarla.");
            }
        }

        RegionCorporal region;
        try
        {
            region = new RegionCorporal(
                id: id ?? Guid.NewGuid(),
                region: input.Region,
                orden: input.Orden);
        }
        catch (ArgumentException exception)
        {
            throw new AdministrationException(exception.Message);
        }

        if (!id.HasValue)
        {
            await store.CreateAsync(region, actor.UserId);
            return;
        }

        await store.UpdateAsync(region, input.Version, actor.UserId);
    }

    private async Task<ActorSession> AuthorizeAsync(bool write)
    {
        var actor = await currentActor.GetAsync();
        var access = await administration.GetAccessAsync(actor);

        AdministrationRules.RequirePermission(access, PermissionNames.CatalogsRead);

        if (write)
        {
            AdministrationRules.RequirePermission(access, PermissionNames.CatalogsManage);
        }

        return actor;
    }
}