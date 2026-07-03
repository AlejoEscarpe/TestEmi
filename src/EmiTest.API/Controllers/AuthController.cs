using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EmiTest.API.DTOs;

namespace EmiTest.API.Controllers
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

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            // Simulación de registro exitoso para propósitos de la prueba técnica 
            return Ok(new { Message = $"User {dto.Username} registered successfully with role {dto.Role}." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            // Simulación de validación de credenciales y asignación de roles 
            string role = "User";

            if (dto.Username.ToLower() == "admin")
            {
                role = "Admin"; // Asigna Rol Admin si el usuario ingresado es admin 
            }

            // Generación del Token JWT
            var token = GenerateJwtToken(dto.Username, role);

            return Ok(new { Token = token, ExpiresInMinutes = _configuration.GetValue<int>("Jwt:DurationInMinutes") });
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role) // Inyección del Rol en el token 
                }),
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("DurationInMinutes")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}