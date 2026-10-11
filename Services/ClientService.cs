using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            return await _context.Clients
                .AsNoTracking()
                .OrderBy(client => client.Name)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                .AsNoTracking()
                .Include(client => client.Contracts)
                .FirstOrDefaultAsync(client => client.ClientId == id);
        }

        public async Task<Client> CreateAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client;
        }

        public async Task<bool> UpdateAsync(Client client)
        {
            if (!await ExistsAsync(client.ClientId))
            {
                return false;
            }

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = await _context.Clients
                .Include(item => item.Contracts)
                .FirstOrDefaultAsync(item => item.ClientId == id);

            if (client == null || client.Contracts.Count > 0)
            {
                return false;
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Clients
                .AnyAsync(client => client.ClientId == id);
        }
    }
}