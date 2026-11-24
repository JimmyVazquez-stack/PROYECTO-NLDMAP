using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ITipoCabelloStore
{
    Task<PageResult<TipoCabelloRow>> GetPageAsync(TipoCabelloQuery query);
    Task CreateAsync(TipoCabello item, string actorId);
    Task UpdateAsync(TipoCabello item, string actorId);
    Task DeleteAsync(Guid id);
}
