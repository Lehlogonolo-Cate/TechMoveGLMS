using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceRequestService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync(
            ServiceRequestStatus? status = null,
            int? contractId = null,
            string? searchTerm = null)
        {
            AddAuthorizationHeader();

            var parameters = new List<string>();

            if (status.HasValue)
            {
                parameters.Add(
                    $"status={(int)status.Value}");
            }

            if (contractId.HasValue)
            {
                parameters.Add(
                    $"contractId={contractId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                parameters.Add(
                    $"search={Uri.EscapeDataString(searchTerm)}");
            }

            var endpoint = "api/ServiceRequests";

            if (parameters.Count > 0)
            {
                endpoint += "?" + string.Join("&", parameters);
            }

            var requests = await _httpClient
                .GetFromJsonAsync<List<ServiceRequest>>(endpoint);

            return requests ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/ServiceRequests/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<ServiceRequest>();
        }

        public async Task<IEnumerable<Contract>>
            GetActiveContractsAsync()
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/Contracts?status={(int)ContractStatus.Active}");

            response.EnsureSuccessStatusCode();

            var contracts = await response.Content
                .ReadFromJsonAsync<List<Contract>>();

            var today = DateTime.Today;

            return (contracts ?? new List<Contract>())
                .Where(contract =>
                    contract.Status == ContractStatus.Active &&
                    contract.StartDate.Date <= today &&
                    contract.EndDate.Date >= today)
                .OrderBy(contract => contract.Client?.Name)
                .ThenBy(contract => contract.ServiceLevel)
                .ToList();
        }

        public async Task<bool> IsContractActiveAsync(
            int contractId)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/Contracts/{contractId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            var contract = await response.Content
                .ReadFromJsonAsync<Contract>();

            if (contract == null)
            {
                return false;
            }

            var today = DateTime.Today;

            return contract.Status == ContractStatus.Active &&
                   contract.StartDate.Date <= today &&
                   contract.EndDate.Date >= today;
        }

        public async Task CreateAsync(
            ServiceRequest serviceRequest)
        {
            AddAuthorizationHeader();

            if (!await IsContractActiveAsync(
                    serviceRequest.ContractId))
            {
                throw new InvalidOperationException(
                    "Service requests can only be created for active contracts.");
            }

            serviceRequest.CreatedAt = DateTime.UtcNow;

            var response = await _httpClient.PostAsJsonAsync(
                "api/ServiceRequests",
                serviceRequest);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(
            ServiceRequest serviceRequest)
        {
            AddAuthorizationHeader();

            if (!await IsContractActiveAsync(
                    serviceRequest.ContractId))
            {
                throw new InvalidOperationException(
                    "Service requests can only be linked to active contracts.");
            }

            var response = await _httpClient.PutAsJsonAsync(
                $"api/ServiceRequests/{serviceRequest.ServiceRequestId}",
                serviceRequest);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException(
                    "The service request could not be found.");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(
                $"api/ServiceRequests/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/ServiceRequests/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("JwtToken");

            _httpClient.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new AuthenticationHeaderValue(
                        "Bearer",
                        token);
        }
    }
}
