using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ITipoSenaStore
{
    Task<PageResult<TipoSenaRow>>GetPageAsync(TipoSenaQuery query);
    Task<TipoSenaRow?> GetAsync(Guid id);
    Task CreateAsync(TipoSena tipoSena, string actorId);
    Task UpdateAsync(TipoSena tipoSena, string expectedVersion, string actorId);
}