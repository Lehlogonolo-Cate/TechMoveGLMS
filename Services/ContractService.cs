using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _context;

        public ContractService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Contract>> GetAllAsync(
            ContractStatus? status = null,
            int? clientId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<Contract> query = _context.Contracts
                .Include(contract => contract.Client)
                .Include(contract => contract.ServiceRequests);

            if (status.HasValue)
            {
                query = query.Where(contract =>
                    contract.Status == status.Value);
            }

            if (clientId.HasValue)
            {
                query = query.Where(contract =>
                    contract.ClientId == clientId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(contract =>
                    contract.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(contract =>
                    contract.EndDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(contract => contract.StartDate)
                .ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(contract => contract.Client)
                .Include(contract => contract.ServiceRequests)
                .FirstOrDefaultAsync(contract =>
                    contract.ContractId == id);
        }

        public async Task CreateAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Contract? contract = await _context.Contracts
                .Include(item => item.ServiceRequests)
                .FirstOrDefaultAsync(item =>
                    item.ContractId == id);

            if (contract == null)
            {
                return;
            }

            if (contract.ServiceRequests.Any())
            {
                throw new InvalidOperationException(
                    "A contract with service requests cannot be deleted.");
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Contracts
                .AnyAsync(contract =>
                    contract.ContractId == id);
        }
    }
}
