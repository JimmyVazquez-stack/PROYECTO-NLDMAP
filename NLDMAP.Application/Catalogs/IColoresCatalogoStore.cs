using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IColoresCatalogoStore
{
    Task<PageResult<ColoresCatalogoRow>> GetPageAsync(ColoresCatalogoQuery query);
    Task CreateAsync(ColoresCatalogo item, string actorId);
    Task UpdateAsync(ColoresCatalogo item, string actorId);
    Task DeleteAsync(Guid id);
}
