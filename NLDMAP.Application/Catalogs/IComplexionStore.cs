using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IComplexionStore
{
    Task<PageResult<ComplexionRow>> GetPageAsync(ComplexionQuery query);
    Task CreateAsync(Complexion item, string actorId);
    Task UpdateAsync(Complexion item, string actorId);
    Task DeleteAsync(Guid id);
}
