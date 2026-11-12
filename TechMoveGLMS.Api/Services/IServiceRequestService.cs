using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Services
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync(
            string? search = null,
            int? contractId = null,
            ServiceRequestStatus? status = null);

        Task<ServiceRequest?> GetByIdAsync(int id);

        Task<ServiceRequest?> CreateAsync(
            ServiceRequest serviceRequest);

        Task<bool> UpdateAsync(
            int id,
            ServiceRequest serviceRequest);

        Task<bool> DeleteAsync(int id);

        Task<bool> ContractExistsAsync(int contractId);
    }
}
