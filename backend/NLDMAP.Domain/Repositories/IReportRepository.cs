using NLDMAP.Domain.Entities;
namespace NLDMAP.Domain.Repositories;

public interface IReportRepository
{
    Task<Report> GetByIdAsync(Guid id);
    Task<Report> GetByFolioAsync(string folio);
    Task AddAsync(Report report);
    Task UpdateAsync(Report report);
}