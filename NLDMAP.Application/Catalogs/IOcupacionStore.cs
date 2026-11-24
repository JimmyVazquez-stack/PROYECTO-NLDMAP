using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IOcupacionStore
{
    Task<PageResult<OcupacionRow>> GetPageAsync(OcupacionQuery query);
    Task CreateAsync(Ocupacion item, string actorId);
    Task UpdateAsync(Ocupacion item, string actorId);
    Task DeleteAsync(Guid id);
}
