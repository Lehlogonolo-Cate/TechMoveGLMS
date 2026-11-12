using TechMoveGLMS.Api.Models;
using TechMoveGLMS.Api.Repositories;

namespace TechMoveGLMS.Api.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;

        public ClientService(IClientRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(
            string? search = null,
            string? region = null)
        {
            return await _repository.GetAllAsync(
                search,
                region);
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Client> CreateAsync(Client client)
        {
            return await _repository.CreateAsync(client);
        }

        public async Task<bool> UpdateAsync(
            int id,
            Client client)
        {
            Client? existingClient =
                await _repository.GetByIdAsync(id);

            if (existingClient is null)
            {
                return false;
            }

            existingClient.Name = client.Name;
            existingClient.ContactDetails =
                client.ContactDetails;
            existingClient.Region = client.Region;

            await _repository.UpdateAsync(existingClient);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Client? client =
                await _repository.GetByIdAsync(id);

            if (client is null)
            {
                return false;
            }

            await _repository.DeleteAsync(client);

            return true;
        }
    }
}
