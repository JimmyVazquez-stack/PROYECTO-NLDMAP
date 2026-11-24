using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ITipoPrendaStore
{
    Task<PageResult<TipoPrendaRow>> GetPageAsync(TipoPrendaQuery query);
    Task<TipoPrendaRow?> GetAsync(Guid id);
    Task CreateAsync(TipoVestimenta tipoPrenda, string actorId);
    Task UpdateAsync(TipoVestimenta tipoPrenda, string actorId);
    Task DeleteAsync(Guid id);
}
