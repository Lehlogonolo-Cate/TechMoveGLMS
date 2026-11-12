using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Api.Data;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync(
            ContractStatus? status,
            string? serviceLevel,
            int? clientId)
        {
            IQueryable<Contract> query = _context.Contracts
                .AsNoTracking()
                .Include(contract => contract.Client);

            if (status.HasValue)
            {
                query = query.Where(
                    contract => contract.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(serviceLevel))
            {
                query = query.Where(contract =>
                    contract.ServiceLevel.Contains(serviceLevel));
            }

            if (clientId.HasValue)
            {
                query = query.Where(
                    contract => contract.ClientId == clientId.Value);
            }

            return await query
                .OrderByDescending(contract => contract.StartDate)
                .ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .AsNoTracking()
                .Include(contract => contract.Client)
                .Include(contract => contract.ServiceRequests)
                .FirstOrDefaultAsync(
                    contract => contract.ContractId == id);
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return contract;
        }

        public async Task<bool> UpdateStatusAsync(
            int id,
            ContractStatus status)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract is null)
            {
                return false;
            }

            contract.Status = status;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClientExistsAsync(int clientId)
        {
            return await _context.Clients.AnyAsync(
                client => client.ClientId == clientId);
        }
    }
}