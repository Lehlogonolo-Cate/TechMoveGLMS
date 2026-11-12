using TechMoveGLMS.Api.DTOs;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Services
{
    public interface IContractService
    {
        Task<IEnumerable<Contract>> GetAllAsync(
            ContractStatus? status,
            string? serviceLevel,
            int? clientId);

        Task<Contract?> GetByIdAsync(int id);

        Task<(Contract? Contract, string? Error)> CreateAsync(
            CreateContractDto dto);

        Task<(bool Updated, bool NotFound, string? Error)>
            UpdateStatusAsync(
                int id,
                UpdateContractStatusDto dto);
    }
}