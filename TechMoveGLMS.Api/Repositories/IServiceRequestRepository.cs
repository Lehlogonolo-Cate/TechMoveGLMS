using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Repositories
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync(
            string? search = null,
            int? contractId = null,
            ServiceRequestStatus? status = null);

        Task<ServiceRequest?> GetByIdAsync(int id);

        Task<ServiceRequest> CreateAsync(
            ServiceRequest serviceRequest);

        Task UpdateAsync(ServiceRequest serviceRequest);

        Task DeleteAsync(ServiceRequest serviceRequest);

        Task<bool> ExistsAsync(int id);

        Task<bool> ContractExistsAsync(int contractId);
    }
}
