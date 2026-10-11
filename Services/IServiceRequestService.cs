using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync(
            ServiceRequestStatus? status = null,
            int? contractId = null,
            string? searchTerm = null);

        Task<ServiceRequest?> GetByIdAsync(int id);

        Task<IEnumerable<Contract>> GetActiveContractsAsync();

        Task<bool> IsContractActiveAsync(int contractId);

        Task CreateAsync(ServiceRequest serviceRequest);

        Task UpdateAsync(ServiceRequest serviceRequest);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}