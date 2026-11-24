using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class ComplexionService(CatalogAccess access, IComplexionStore store)
{
    public async Task<PageResult<ComplexionRow>> GetPageAsync(ComplexionQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(ComplexionInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Complexion item;
        try
        {
            item = new Complexion(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreComplexion, "Complexión", 120));
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
