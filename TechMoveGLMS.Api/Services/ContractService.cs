using TechMoveGLMS.Api.DTOs;
using TechMoveGLMS.Api.Models;
using TechMoveGLMS.Api.Repositories;

namespace TechMoveGLMS.Api.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repository;

        public ContractService(IContractRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Contract>> GetAllAsync(
            ContractStatus? status,
            string? serviceLevel,
            int? clientId)
        {
            return _repository.GetAllAsync(
                status,
                serviceLevel,
                clientId);
        }

        public Task<Contract?> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        public async Task<(Contract? Contract, string? Error)>
            CreateAsync(CreateContractDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
            {
                return (
                    null,
                    "The end date must be later than the start date.");
            }

            if (!Enum.IsDefined(dto.Status))
            {
                return (null, "The contract status is invalid.");
            }

            var clientExists =
                await _repository.ClientExistsAsync(dto.ClientId);

            if (!clientExists)
            {
                return (
                    null,
                    $"Client with ID {dto.ClientId} was not found.");
            }

            var contract = new Contract
            {
                ClientId = dto.ClientId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ServiceLevel = dto.ServiceLevel.Trim()
            };

            var createdContract =
                await _repository.CreateAsync(contract);

            return (createdContract, null);
        }

        public async Task<
            (bool Updated, bool NotFound, string? Error)>
            UpdateStatusAsync(
                int id,
                UpdateContractStatusDto dto)
        {
            if (!Enum.IsDefined(dto.Status))
            {
                return (
                    false,
                    false,
                    "The contract status is invalid.");
            }

            var updated = await _repository.UpdateStatusAsync(
                id,
                dto.Status);

            if (!updated)
            {
                return (false, true, null);
            }

            return (true, false, null);
        }
    }
}
