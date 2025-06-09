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
    public class AuthenticateCompany : IAuthenticateCompany
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticateCompany(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool isAuthenticated, string? errorMessage, int? statusCode)> AuthenticateAsync(string email, string senha, string ipAddress)
        {
            var loginAttempt = await _context.LoginAttempt.FirstOrDefaultAsync(x => x.IPAddress == ipAddress);
            if (loginAttempt?.LockoutEnd > DateTime.UtcNow)
            {
                var hora = loginAttempt.LockoutEnd.Value.ToLocalTime().ToString("HH:mm");
                return (false, $"Este IP está temporariamente bloqueado. Tente novamente após {hora}.", 423);
            }

            var user = await GetUser(email);
            if (user == null)
                return (false, "Usuário ou senha inválidos.", 401);

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));

            if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            {
                if (loginAttempt == null)
                {
                    loginAttempt = new LoginAttempt
                    {
                        IPAddress = ipAddress,
                        FailedAttempts = 1
                    };
                    _context.LoginAttempt.Add(loginAttempt);
                }
                else
                {
                    loginAttempt.FailedAttempts++;

                    if (loginAttempt.FailedAttempts >= 5)
                    {
                        loginAttempt.LockoutEnd = DateTime.UtcNow.AddHours(1);
                        loginAttempt.FailedAttempts = 0;
                    }
                }

                await _context.SaveChangesAsync();
                return (false, "Usuário ou senha inválidos.", 401);
            }

            if (loginAttempt != null)
            {
                loginAttempt.FailedAttempts = 0;
                loginAttempt.LockoutEnd = null;
            }

            await _context.SaveChangesAsync();
            return (true, null, 200);
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
        public async Task<Company> GetUserByEmail(string email)
        {
            return await GetUser(email);
        }

        public async Task<bool> UserExists(string email)
        {
            var user = await GetUser(email);
            return user != null;
        }
        private async Task<Company?> GetUser(string email)
        {
            return await _context.Company
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }
    }
}
