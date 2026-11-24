using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class ParentescoService(CatalogAccess access, IParentescoStore store)
{
    public async Task<PageResult<ParentescoRow>> GetPageAsync(ParentescoQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(ParentescoInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        Parentesco item;
        try
        {
            item = new Parentesco(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreParentesco, "Parentesco", 120));
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
