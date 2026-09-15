using NLDMAP.Domain.Interfaces;
using NLDMAP.Domain.Entities;

namespace NLDMAP.Infrastructure.Repositories
{
    public class EfUserRepository : IUserRepository
    {
        //Inyectar despues aqui el Nldmaodbcontext
        public async Task<User?> GetByEmailAsync(string email)
        {
           //falta implementar busqueda real en postgresql
            return await Task.FromResult<User?>(null);
        }
        
        public async Task AddSync(User user)
        {
            //implementar insercion real en postgresql    
            await Task.CompletedTask;
        }
    }
}