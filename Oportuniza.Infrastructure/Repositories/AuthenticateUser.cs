using Microsoft.Extensions.Configuration;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Cryptography;
using System.Text;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

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

        public async Task<bool> AuthenticateAsync(string email, string senha)
        {

            var user = await _context.User.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            if (user == null) return false;

            if (user.PasswordSalt.Length != 128 || user.PasswordHash.Length != 64)
            {
                Console.WriteLine("Tamanho inválido de salt ou hash.");
                return false;
            }

            using (var hmac = new HMACSHA512(user.PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != user.PasswordHash[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string GenerateToken(Guid id, string email, bool isACompany)
        {
            var claims = new[]
                       {
                new Claim("id", id.ToString()),
                new Claim("email", email),
                new Claim("isACompany", isACompany.ToString().ToLower()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var privateKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:secretKey"]));

            var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(10);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["jwt:issuer"],
                audience: _configuration["jwt:audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.User.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<bool> UserExists(string email)
        {
            var user = await _context.User.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            if (user == null) return false;
            return true;
        }
    }
}
