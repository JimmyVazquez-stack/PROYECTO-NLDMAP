using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class NacionalidadService(CatalogAccess access, INacionalidadStore store)
{
    public async Task<PageResult<NacionalidadRow>> GetPageAsync(NacionalidadQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(NacionalidadInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Nacionalidad item;
        try
        {
            item = new Nacionalidad(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreNacionalidad, "Nacionalidad", 120));
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
