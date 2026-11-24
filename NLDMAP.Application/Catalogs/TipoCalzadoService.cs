using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class TipoCalzadoService(CatalogAccess access, ITipoCalzadoStore store)
{
    public async Task<PageResult<TipoCalzadoRow>> GetPageAsync(TipoCalzadoQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(TipoCalzadoInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        TipoCalzado item;
        try
        {
            item = new TipoCalzado(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreCalzado, "Tipo de calzado", 120));
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
