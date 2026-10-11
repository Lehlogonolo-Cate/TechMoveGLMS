using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync(
            ServiceRequestStatus? status = null,
            int? contractId = null,
            string? searchTerm = null)
        {
            var query = _context.ServiceRequests
                .Include(request => request.Contract)
                .ThenInclude(contract => contract!.Client)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(request =>
                    request.Status == status.Value);
            }

            if (contractId.HasValue)
            {
                query = query.Where(request =>
                    request.ContractId == contractId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(request =>
                    request.Description.Contains(searchTerm) ||
                    request.Contract!.ServiceLevel.Contains(searchTerm) ||
                    request.Contract.Client!.Name.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(request => request.CreatedAt)
                .ToListAsync();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(request => request.Contract)
                .ThenInclude(contract => contract!.Client)
                .FirstOrDefaultAsync(request =>
                    request.ServiceRequestId == id);
        }

        public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
        {
            var today = DateTime.Today;

            return await _context.Contracts
                .Include(contract => contract.Client)
                .Where(contract =>
                    contract.Status == ContractStatus.Active &&
                    contract.StartDate <= today &&
                    contract.EndDate >= today)
                .OrderBy(contract => contract.Client!.Name)
                .ThenBy(contract => contract.ServiceLevel)
                .ToListAsync();
        }

        public async Task<bool> IsContractActiveAsync(int contractId)
        {
            var today = DateTime.Today;

            return await _context.Contracts.AnyAsync(contract =>
                contract.ContractId == contractId &&
                contract.Status == ContractStatus.Active &&
                contract.StartDate <= today &&
                contract.EndDate >= today);
        }

        public async Task CreateAsync(ServiceRequest serviceRequest)
        {
            if (!await IsContractActiveAsync(serviceRequest.ContractId))
            {
                throw new InvalidOperationException(
                    "Service requests can only be created for active contracts.");
            }

            serviceRequest.CreatedAt = DateTime.UtcNow;

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ServiceRequest serviceRequest)
        {
            if (!await IsContractActiveAsync(serviceRequest.ContractId))
            {
                throw new InvalidOperationException(
                    "Service requests can only be linked to active contracts.");
            }

            var existingRequest =
                await _context.ServiceRequests.FindAsync(
                    serviceRequest.ServiceRequestId);

            if (existingRequest == null)
            {
                throw new KeyNotFoundException(
                    "The service request could not be found.");
            }

            existingRequest.ContractId = serviceRequest.ContractId;
            existingRequest.Description = serviceRequest.Description;
            existingRequest.CostUsd = serviceRequest.CostUsd;
            existingRequest.ExchangeRate = serviceRequest.ExchangeRate;
            existingRequest.Cost = serviceRequest.Cost;
            existingRequest.Status = serviceRequest.Status;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var serviceRequest =
                await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
            {
                return;
            }

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ServiceRequests.AnyAsync(request =>
                request.ServiceRequestId == id);
        }
    }
}
