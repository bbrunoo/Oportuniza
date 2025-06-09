using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IAuthenticateCompany
    {
        Task<(bool isAuthenticated, string? errorMessage, int? statusCode)> AuthenticateAsync(string email, string senha, string ipAddress);
        Task<bool> UserExists(string email);
        public string GenerateToken(Guid id, string email, string name);
        public Task<Company> GetUserByEmail(string email);

    }
}
