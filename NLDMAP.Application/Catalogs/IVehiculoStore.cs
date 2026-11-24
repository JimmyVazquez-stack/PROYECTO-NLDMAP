using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IVehiculoStore
{
    Task<PageResult<VehiculoRow>> GetPageAsync(VehiculoQuery query);
    Task CreateAsync(Vehiculo item, string actorId);
    Task UpdateAsync(Vehiculo item, string actorId);
    Task DeleteAsync(Guid id);
}
