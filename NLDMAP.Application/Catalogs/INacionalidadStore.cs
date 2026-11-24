using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface INacionalidadStore
{
    Task<PageResult<NacionalidadRow>> GetPageAsync(NacionalidadQuery query);
    Task CreateAsync(Nacionalidad item, string actorId);
    Task UpdateAsync(Nacionalidad item, string actorId);
    Task DeleteAsync(Guid id);
}
