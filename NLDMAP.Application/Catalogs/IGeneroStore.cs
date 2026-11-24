using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IGeneroStore
{
    Task<PageResult<GeneroRow>> GetPageAsync(GeneroQuery query);
    Task CreateAsync(Genero item, string actorId);
    Task UpdateAsync(Genero item, string actorId);
    Task DeleteAsync(Guid id);
}
