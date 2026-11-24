using NLDMAP.Application.Administration;
using NLDMAP.Domain.Reports;

namespace NLDMAP.Application.Reports;

public interface IReportStore
{
    Task<IReadOnlyList<AnalysisReport>> GetAnalysisAsync(bool includeLocalExamples);
    Task CreateAsync(Reporte report);
    Task<PageResult<ReportRow>> GetPageAsync(string? authorId, int page);
    Task<IReadOnlyList<ReportMapPoint>> GetMapAsync(bool publicOnly, string? authorId);
    Task<Reporte?> GetAsync(string id, string? authorId);
}
