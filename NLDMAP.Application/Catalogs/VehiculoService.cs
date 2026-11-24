using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class VehiculoService(CatalogAccess access, IVehiculoStore store)
{
    public async Task<PageResult<VehiculoRow>> GetPageAsync(VehiculoQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(VehiculoInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Vehiculo item;
        try
        {
            item = new Vehiculo(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.Marca, "Marca", 120));
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
