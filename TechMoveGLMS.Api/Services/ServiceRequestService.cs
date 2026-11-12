using TechMoveGLMS.Api.Models;
using TechMoveGLMS.Api.Repositories;

namespace TechMoveGLMS.Api.Services
{
    public class ServiceRequestService
        : IServiceRequestService
    {
        private readonly IServiceRequestRepository _repository;

        public ServiceRequestService(
            IServiceRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync(
            string? search = null,
            int? contractId = null,
            ServiceRequestStatus? status = null)
        {
            return await _repository.GetAllAsync(
                search,
                contractId,
                status);
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<ServiceRequest?> CreateAsync(
            ServiceRequest serviceRequest)
        {
            bool contractExists =
                await _repository.ContractExistsAsync(
                    serviceRequest.ContractId);

            if (!contractExists)
            {
                return null;
            }

            serviceRequest.Cost = Math.Round(
                serviceRequest.CostUsd *
                serviceRequest.ExchangeRate,
                2);

            serviceRequest.CreatedAt = DateTime.UtcNow;

            return await _repository.CreateAsync(serviceRequest);
        }

        public async Task<bool> UpdateAsync(
            int id,
            ServiceRequest serviceRequest)
        {
            ServiceRequest? existingRequest =
                await _repository.GetByIdAsync(id);

            if (existingRequest is null)
            {
                return false;
            }

            bool contractExists =
                await _repository.ContractExistsAsync(
                    serviceRequest.ContractId);

            if (!contractExists)
            {
                return false;
            }

            existingRequest.ContractId =
                serviceRequest.ContractId;

            existingRequest.Description =
                serviceRequest.Description;

            existingRequest.CostUsd =
                serviceRequest.CostUsd;

            existingRequest.ExchangeRate =
                serviceRequest.ExchangeRate;

            existingRequest.Cost = Math.Round(
                serviceRequest.CostUsd *
                serviceRequest.ExchangeRate,
                2);

            existingRequest.Status =
                serviceRequest.Status;

            await _repository.UpdateAsync(existingRequest);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            ServiceRequest? serviceRequest =
                await _repository.GetByIdAsync(id);

            if (serviceRequest is null)
            {
                return false;
            }

            await _repository.DeleteAsync(serviceRequest);

            return true;
        }

        public async Task<bool> ContractExistsAsync(
            int contractId)
        {
            return await _repository.ContractExistsAsync(
                contractId);
        }
    }
}
