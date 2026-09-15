using NLDMAP.Domain.Entities;

namespace NLDMAP.Domain.Interfaces;

    public interface IUserRepository
    {
        //busca un usuario por email
        Task<User?> GetByEmailAsync(string email);

        //agrega un nuevo ciudadano
        Task AddSync(User user);
    }
