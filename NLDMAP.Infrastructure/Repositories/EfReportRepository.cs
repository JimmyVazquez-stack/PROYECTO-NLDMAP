using System;
using System.Threading.Tasks;
using NLDMAP.Domain.Entities;
using NLDMAP.Domain.Repositories;

namespace NLDMAP.Infrastructure.Repositories
{

    public class EfReportRepository : IReportRepository
    {
        public  Task AddAsync(Report report)
        {
            //Implementacion en Postgresql, mongodb y neo4j
            return Task.CompletedTask;
        }

        public Task<Report> GetByFolioAsync(string folio)
        {
            //implementar busqueda real 
            return  Task.FromResult<Report>(null!);
        }

        public Task<Report> GetByIdAsync(Guid id)
        {
            return  Task.FromResult<Report>(null!);
        }

        public Task UpdateAsync(Report report)
        {
            //implementar actualizacion real
            return Task.CompletedTask;
        }
    }
}