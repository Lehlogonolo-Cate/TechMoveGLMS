using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TechMoveGLMS.Tests.Integration
{
    public class AuthApiTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthApiTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            var response =
                await _client.GetAsync("/api/Clients");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsJwtToken()
        {
            var loginRequest = new
            {
                username = "admin",
                password = "TechMove@2026"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            var json = await response.Content
                .ReadFromJsonAsync<JsonElement>();

            Assert.True(json.TryGetProperty("token", out var token));
            Assert.False(
                string.IsNullOrWhiteSpace(token.GetString()));
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var loginRequest = new
            {
                username = "invalid-user",
                password = "invalid-password"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
}
