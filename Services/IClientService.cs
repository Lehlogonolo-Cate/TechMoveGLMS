using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public interface IClientService
    {
        Task<List<Client>> GetAllAsync();

        Task<Client?> GetByIdAsync(int id);

        Task<Client> CreateAsync(Client client);

        Task<bool> UpdateAsync(Client client);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
