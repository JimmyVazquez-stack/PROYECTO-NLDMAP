using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ITipoCalzadoStore
{
    Task<PageResult<TipoCalzadoRow>> GetPageAsync(TipoCalzadoQuery query);
    Task CreateAsync(TipoCalzado item, string actorId);
    Task UpdateAsync(TipoCalzado item, string actorId);
    Task DeleteAsync(Guid id);
}
