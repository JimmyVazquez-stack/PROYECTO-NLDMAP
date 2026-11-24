using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class TipoCabelloService(CatalogAccess access, ITipoCabelloStore store)
{
    public async Task<PageResult<TipoCabelloRow>> GetPageAsync(TipoCabelloQuery query)
    {
        await access.AuthorizeAsync(write: false);
        return await store.GetPageAsync(query);
    }

    public async Task SaveAsync(TipoCabelloInput input)
    {
        var actor = await access.AuthorizeAsync(write: true);
        TipoCabello item;
        try
        {
            item = new TipoCabello(input.Id ?? Guid.NewGuid(),
                CatalogInput.Required(input.NombreCabello, "Tipo de cabello", 120),
                CatalogInput.Required(input.LargoCabello, "Largo del cabello", 120));
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
