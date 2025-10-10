using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IAuthenticateUser
    {
        Task<(bool isAuthenticated, string? errorMessage, int? statusCode)> AuthenticateAsync(string email, string senha, string ipAddress);
        Task<bool> UserExists(string email);
        string GenerateToken(Guid id, string email, string name);
        string GenerateToken(Guid id, string email, string name, Guid? activeCompanyId, string? companyRole);
        public Task<User> GetUserByEmail(string email);
    }
}
