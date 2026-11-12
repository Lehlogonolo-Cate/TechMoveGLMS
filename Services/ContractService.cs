using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class ContractService : IContractService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContractService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Contract>> GetAllAsync(
            ContractStatus? status = null,
            int? clientId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            AddAuthorizationHeader();

            var parameters = new List<string>();

            if (status.HasValue)
            {
                parameters.Add(
                    $"status={(int)status.Value}");
            }

            if (clientId.HasValue)
            {
                parameters.Add(
                    $"clientId={clientId.Value}");
            }

            if (startDate.HasValue)
            {
                parameters.Add(
                    $"startDate={startDate.Value:yyyy-MM-dd}");
            }

            if (endDate.HasValue)
            {
                parameters.Add(
                    $"endDate={endDate.Value:yyyy-MM-dd}");
            }

            var endpoint = "api/Contracts";

            if (parameters.Count > 0)
            {
                endpoint += "?" + string.Join("&", parameters);
            }

            var contracts =
                await _httpClient.GetFromJsonAsync<List<Contract>>(
                    endpoint);

            return contracts ?? new List<Contract>();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/Contracts/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<Contract>();
        }

        public async Task CreateAsync(Contract contract)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync(
                "api/Contracts",
                contract);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(Contract contract)
        {
            AddAuthorizationHeader();

            var statusUpdate = new
            {
                status = contract.Status
            };

            var response = await _httpClient.PatchAsJsonAsync(
                $"api/Contracts/{contract.ContractId}/status",
                statusUpdate);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException(
                    $"Contract with ID {contract.ContractId} was not found.");
            }

            response.EnsureSuccessStatusCode();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotSupportedException(
                "Deleting contracts is not supported by the Contracts API.");
        }

        public async Task<bool> ExistsAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/Contracts/{id}");

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
