using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Oportuniza.Infrastructure.Repositories
{
    public class AuthenticateUser : IAuthenticateUser
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticateUser(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Task<(bool isAuthenticated, string? errorMessage, int? statusCode)> AuthenticateAsync(string email, string senha, string ipAddress)
        {
            throw new NotImplementedException();
        }

        public string GenerateToken(Guid id, string email, string name)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretKey = _configuration["jwt:secretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT secret key not found in configuration.");

            var privateKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(
                          double.TryParse(_configuration["jwt:expiryMinutes"], out var minutes) ? minutes : 60
                      );

            var token = new JwtSecurityToken(
              issuer: _configuration["jwt:issuer"],
              audience: _configuration["jwt:audience"],
              claims: claims,
              expires: expiration,
              signingCredentials: credentials
          );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task GetBlockedIp(string ip)
        {
            await _context.LoginAttempt.FirstOrDefaultAsync(x => x.IPAddress == ip);
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await GetUser(email);
        }

        public async Task<bool> UserExists(string email)
        {
            var user = await GetUser(email);
            return user != null;
        }
        private async Task<User?> GetUser(string email)
        {
            return await _context.User
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }
    }
}
