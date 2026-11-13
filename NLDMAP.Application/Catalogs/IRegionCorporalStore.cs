using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface IRegionCorporalStore
{
    Task<PageResult<RegionCorporalRow>> GetPageAsync(RegionCorporalQuery query);
    Task<RegionCorporalRow?> GetAsync(Guid id);
    Task CreateAsync(RegionCorporal region, string actorId);
    Task UpdateAsync(RegionCorporal region, string expectedVersion, string actorId);
}