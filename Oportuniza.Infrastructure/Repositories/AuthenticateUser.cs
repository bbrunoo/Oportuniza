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

        //public string GenerateToken(Guid id, string email, string name, Guid? activeCompanyId, string? companyRole)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
        //        new Claim(ClaimTypes.Email, email),
        //        new Claim(ClaimTypes.Name, name),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    if (activeCompanyId.HasValue)
        //        claims.Add(new Claim("activeCompanyId", activeCompanyId.Value.ToString()));

        //    if (!string.IsNullOrEmpty(companyRole))
        //        claims.Add(new Claim("companyRole", companyRole));

        //    var secretKey = _configuration["jwt:secretKey"];
        //    if (string.IsNullOrEmpty(secretKey))
        //        throw new InvalidOperationException("JWT secret key not found in configuration.");

        //    var privateKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        //    var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

        //    var expiration = DateTime.UtcNow.AddMinutes(
        //        double.TryParse(_configuration["jwt:expiryMinutes"], out var minutes) ? minutes : 60
        //    );

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["jwt:issuer"],
        //        audience: _configuration["jwt:audience"],
        //        claims: claims,
        //        expires: expiration,
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}


        public string GenerateToken(Guid id, string email, string name, Guid? activeCompanyId, string? companyRole)
        {
            // 1. Definição das Claims Base
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            // 2. Adiciona a Claim de Empresa Ativa (Tenant Context)
            // Usaremos a chave 'company_id' (minúsculas) para padronização e fácil leitura.
            if (activeCompanyId.HasValue)
                claims.Add(new Claim("company_id", activeCompanyId.Value.ToString()));

            // 3. Adiciona a Claim de Papel no Contexto da Empresa (Role)
            // É crucial usar ClaimTypes.Role para integração com a autorização padrão do .NET
            if (!string.IsNullOrEmpty(companyRole))
            {
                // Se a role da empresa já existir, adiciona o novo papel.
                // Se for um sistema de substituição total, remove a claim anterior (não aplicável aqui, mas bom saber).
                claims.Add(new Claim(ClaimTypes.Role, companyRole));
            }

            // 4. Lógica de Geração do Token (mantida inalterada, focando em segurança e expiração)
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
