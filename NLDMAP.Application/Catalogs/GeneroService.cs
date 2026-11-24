using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class GeneroService(CatalogAccess access, IGeneroStore store)
{
    public async Task<PageResult<GeneroRow>> GetPageAsync(GeneroQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(GeneroInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Genero item;
        try
        {
            item = new Genero(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreSexo, "Nombre del sexo", 120));
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
