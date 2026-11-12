using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            AddAuthorizationHeader();

            var clients =
                await _httpClient.GetFromJsonAsync<List<Client>>(
                    "api/Clients");

            return clients ?? new List<Client>();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/Clients/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<Client>();
        }

        public async Task<Client> CreateAsync(Client client)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync(
                "api/Clients",
                client);

            response.EnsureSuccessStatusCode();

            var createdClient = await response.Content
                .ReadFromJsonAsync<Client>();

            return createdClient
                ?? throw new InvalidOperationException(
                    "The API did not return the created client.");
        }

        public async Task<bool> UpdateAsync(Client client)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PutAsJsonAsync(
                $"api/Clients/{client.ClientId}",
                client);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(
                $"api/Clients/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Conflict ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(
                $"api/Clients/{id}");

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