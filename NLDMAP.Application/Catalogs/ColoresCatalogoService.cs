using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class ColoresCatalogoService(CatalogAccess access, IColoresCatalogoStore store)
{
    public async Task<PageResult<ColoresCatalogoRow>> GetPageAsync(ColoresCatalogoQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(ColoresCatalogoInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        ColoresCatalogo item;
        try
        {
            item = new ColoresCatalogo(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.ColorNombre, "Nombre del color", 120),
                CatalogInput.Required(input.ColorHex, "Color HEX", 7));
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
