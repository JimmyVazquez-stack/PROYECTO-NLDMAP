using NLDMAP.Application.Administration;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public interface ISepomexStore
{
    Task<PageResult<AsentamientoPostal>> GetPageAsync(string search, int page);
    Task<ResultadoImportacionPostal> ImportAsync(IReadOnlyList<AsentamientoPostal> rows, string actorId);
}
