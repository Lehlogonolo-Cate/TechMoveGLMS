using System.Net;
using System.Net.Http.Json;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse?> LoginAsync(
            LoginViewModel login)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/login",
                login);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<LoginResponse>();
        }
    }
}