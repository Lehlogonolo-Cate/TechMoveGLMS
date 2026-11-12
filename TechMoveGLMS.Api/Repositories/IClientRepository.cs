using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Repositories
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync(
            string? search = null,
            string? region = null);

        Task<Client?> GetByIdAsync(int id);

        Task<Client> CreateAsync(Client client);

        Task UpdateAsync(Client client);

        Task DeleteAsync(Client client);

        Task<bool> ExistsAsync(int id);
    }
}