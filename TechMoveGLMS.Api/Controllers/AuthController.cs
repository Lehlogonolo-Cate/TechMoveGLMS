using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TechMoveGLMS.Api.DTOs;

namespace TechMoveGLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: /api/auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login(LoginDto dto)
        {
            var configuredUsername =
                _configuration["Authentication:Username"];

            var configuredPassword =
                _configuration["Authentication:Password"];

            if (dto.Username != configuredUsername ||
                dto.Password != configuredPassword)
            {
                return Unauthorized(new
                {
                    message = "The username or password is incorrect."
                });
            }

            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "The JWT key is missing.");

            var expiryMinutes =
                _configuration.GetValue<int>("Jwt:ExpiryMinutes");

            var expiresAt = DateTime.UtcNow.AddMinutes(
                expiryMinutes > 0 ? expiryMinutes : 60);

            var claims = new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    dto.Username),

                new(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),

                new(
                    ClaimTypes.Name,
                    dto.Username),

                new(
                    ClaimTypes.Role,
                    "Administrator")
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenValue =
                new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenValue,
                tokenType = "Bearer",
                expiresAt,
                username = dto.Username,
                role = "Administrator"
            });
        }
    }
}
