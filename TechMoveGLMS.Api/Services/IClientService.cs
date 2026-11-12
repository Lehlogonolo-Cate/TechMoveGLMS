using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Services
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync(
            string? search = null,
            string? region = null);

        Task<Client?> GetByIdAsync(int id);

        Task<Client> CreateAsync(Client client);

        Task<bool> UpdateAsync(int id, Client client);

        Task<bool> DeleteAsync(int id);
    }
}
