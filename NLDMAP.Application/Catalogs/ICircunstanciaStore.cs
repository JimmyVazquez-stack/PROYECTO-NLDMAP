using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ICircunstanciaStore
{
    Task<PageResult<CircunstanciaRow>> GetPageAsync(CircunstanciaQuery query);
    Task CreateAsync(Circunstancia item, string actorId);
    Task UpdateAsync(Circunstancia item, string actorId);
    Task DeleteAsync(Guid id);
}
