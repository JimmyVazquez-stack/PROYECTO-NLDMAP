using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class OcupacionService(CatalogAccess access, IOcupacionStore store)
{
    public async Task<PageResult<OcupacionRow>> GetPageAsync(OcupacionQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(OcupacionInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Ocupacion item;
        try
        {
            item = new Ocupacion(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreOcupacion, "Ocupación", 120),
                CatalogInput.Optional(input.DescripcionOcupacion, "Descripción", 500));
        }
        catch (ArgumentException exception)
        {
            throw new AdministrationException(exception.Message);
        }

        if (input.Id is null)
            await store.CreateAsync(item, actor.UserId);
        else
            await store.UpdateAsync(item, actor.UserId);
    }

    public async Task DeleteAsync(Guid id)
    {
        await access.AuthorizeAsync(write: true);
        await store.DeleteAsync(id);
    }
}
