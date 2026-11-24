using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class DiscapacidadService(CatalogAccess access, IDiscapacidadStore store)
{
    public async Task<PageResult<DiscapacidadRow>> GetPageAsync(DiscapacidadQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(DiscapacidadInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Discapacidad item;
        try
        {
            item = new Discapacidad(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreDiscapacidad, "Discapacidad", 120));
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
