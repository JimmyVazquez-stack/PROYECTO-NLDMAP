using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IParentescoStore
{
    Task<PageResult<ParentescoRow>> GetPageAsync(ParentescoQuery query);
    Task CreateAsync(Parentesco item, string actorId);
    Task UpdateAsync(Parentesco item, string actorId);
    Task DeleteAsync(Guid id);
}
