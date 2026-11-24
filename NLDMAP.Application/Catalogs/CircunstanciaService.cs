using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class CircunstanciaService(CatalogAccess access, ICircunstanciaStore store)
{
    public async Task<PageResult<CircunstanciaRow>> GetPageAsync(CircunstanciaQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(CircunstanciaInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Circunstancia item;
        try
        {
            item = new Circunstancia(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.Tipo, "Tipo", 120),
                CatalogInput.Optional(input.DescripcionCircunstancia, "Descripción", 500));
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
