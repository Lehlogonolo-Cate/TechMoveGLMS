using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Api.Data;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Repositories
{
    public class ServiceRequestRepository
        : IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync(
            string? search = null,
            int? contractId = null,
            ServiceRequestStatus? status = null)
        {
            IQueryable<ServiceRequest> query =
                _context.ServiceRequests
                    .Include(request => request.Contract)
                    .ThenInclude(contract => contract!.Client)
                    .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTerm = search.Trim();

                query = query.Where(request =>
                    request.Description.Contains(searchTerm) ||
                    request.Contract!.ServiceLevel.Contains(searchTerm) ||
                    request.Contract.Client!.Name.Contains(searchTerm));
            }

            if (contractId.HasValue)
            {
                query = query.Where(request =>
                    request.ContractId == contractId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(request =>
                    request.Status == status.Value);
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
                .AsNoTracking()
                .FirstOrDefaultAsync(request =>
                    request.ServiceRequestId == id);
        }

        public async Task<ServiceRequest> CreateAsync(
            ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            return serviceRequest;
        }

        public async Task UpdateAsync(
            ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Update(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ServiceRequests
                .AnyAsync(request =>
                    request.ServiceRequestId == id);
        }

        public async Task<bool> ContractExistsAsync(int contractId)
        {
            return await _context.Contracts
                .AnyAsync(contract =>
                    contract.ContractId == contractId);
        }
    }
}
