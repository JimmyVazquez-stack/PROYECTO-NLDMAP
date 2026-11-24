using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IOrientacionSexualStore
{
    Task<PageResult<OrientacionSexualRow>> GetPageAsync(OrientacionSexualQuery query);
    Task CreateAsync(OrientacionSexual item, string actorId);
    Task UpdateAsync(OrientacionSexual item, string actorId);
    Task DeleteAsync(Guid id);
}
