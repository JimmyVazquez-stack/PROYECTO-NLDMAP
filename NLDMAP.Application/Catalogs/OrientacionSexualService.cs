using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class OrientacionSexualService(CatalogAccess access, IOrientacionSexualStore store)
{
    public async Task<PageResult<OrientacionSexualRow>> GetPageAsync(OrientacionSexualQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(OrientacionSexualInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        OrientacionSexual item;
        try
        {
            item = new OrientacionSexual(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreOrientacionSexual, "Orientación sexual", 120),
                CatalogInput.Optional(input.DescripcionOrientacionSexual, "Descripción", 500));
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
