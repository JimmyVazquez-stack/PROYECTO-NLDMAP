using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IDiscapacidadStore
{
    Task<PageResult<DiscapacidadRow>> GetPageAsync(DiscapacidadQuery query);
    Task CreateAsync(Discapacidad item, string actorId);
    Task UpdateAsync(Discapacidad item, string actorId);
    Task DeleteAsync(Guid id);
}
