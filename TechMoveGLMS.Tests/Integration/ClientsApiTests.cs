using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TechMoveGLMS.Tests.Integration
{
    public class ClientsApiTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ClientsApiTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateClient_ThenReadClient_PreservesData()
        {
            await AuthenticateAsync();

            var uniqueName =
                $"Integration Client {Guid.NewGuid():N}";

            var createRequest = new
            {
                name = uniqueName,
                contactDetails = "integration@test.local",
                region = "Gauteng"
            };

            var createResponse =
                await _client.PostAsJsonAsync(
                    "/api/Clients",
                    createRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var createdClient = await createResponse.Content
                .ReadFromJsonAsync<JsonElement>();

            Assert.True(
                createdClient.TryGetProperty(
                    "clientId",
                    out var clientIdElement));

            var clientId = clientIdElement.GetInt32();

            Assert.True(clientId > 0);

            var getResponse =
                await _client.GetAsync(
                    $"/api/Clients/{clientId}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var returnedClient = await getResponse.Content
                .ReadFromJsonAsync<JsonElement>();

            Assert.Equal(
                clientId,
                returnedClient.GetProperty("clientId").GetInt32());

            Assert.Equal(
                uniqueName,
                returnedClient.GetProperty("name").GetString());

            Assert.Equal(
                "integration@test.local",
                returnedClient
                    .GetProperty("contactDetails")
                    .GetString());

            Assert.Equal(
                "Gauteng",
                returnedClient.GetProperty("region").GetString());
        }

        [Fact]
        public async Task GetClient_WithUnknownId_ReturnsNotFound()
        {
            await AuthenticateAsync();

            var response =
                await _client.GetAsync(
                    "/api/Clients/999999");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        private async Task AuthenticateAsync()
        {
            var loginRequest = new
            {
                username = "admin",
                password = "TechMove@2026"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadFromJsonAsync<JsonElement>();

            var token = json
                .GetProperty("token")
                .GetString();

            Assert.False(string.IsNullOrWhiteSpace(token));

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }
    }
}
