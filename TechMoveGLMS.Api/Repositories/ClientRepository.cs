using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Api.Data;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(
            string? search = null,
            string? region = null)
        {
            IQueryable<Client> query = _context.Clients
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTerm = search.Trim();

                query = query.Where(client =>
                    client.Name.Contains(searchTerm) ||
                    client.ContactDetails.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(region))
            {
                string regionTerm = region.Trim();

                query = query.Where(client =>
                    client.Region.Contains(regionTerm));
            }

            return await query
                .OrderBy(client => client.Name)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(client =>
                    client.ClientId == id);
        }

        public async Task<Client> CreateAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client;
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Clients
                .AnyAsync(client => client.ClientId == id);
        }
    }
}