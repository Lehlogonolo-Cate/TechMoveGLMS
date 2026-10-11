using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public interface IContractService
    {
        Task<List<Contract>> GetAllAsync(
            ContractStatus? status = null,
            int? clientId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<Contract?> GetByIdAsync(int id);

        Task CreateAsync(Contract contract);

        Task UpdateAsync(Contract contract);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
