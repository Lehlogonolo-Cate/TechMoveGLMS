using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Repositories
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync(
            ContractStatus? status,
            string? serviceLevel,
            int? clientId);

        Task<Contract?> GetByIdAsync(int id);

        Task<Contract> CreateAsync(Contract contract);

        Task<bool> UpdateStatusAsync(
            int id,
            ContractStatus status);

        Task<bool> ClientExistsAsync(int clientId);
    }
}