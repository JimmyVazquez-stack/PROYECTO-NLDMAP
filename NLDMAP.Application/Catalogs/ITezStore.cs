using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ITezStore
{
    Task<PageResult<TezRow>> GetPageAsync(TezQuery query);
    Task<TezRow?> GetAsync(Guid id);
    Task CreateAsync(Tez tez, string actorId);
    Task UpdateAsync(Tez tez, string expectedVersion, string actorId);
}