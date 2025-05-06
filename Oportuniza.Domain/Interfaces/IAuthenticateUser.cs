using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IAuthenticateUser
    {
        Task<(bool isAuthenticated, string? errorMessage, int? statusCode)> AuthenticateAsync(string email, string senha, string ipAddress);
        Task<bool> UserExists(string email);
        public string GenerateToken(Guid id, string email, bool isACompany, string name);
        public Task<User> GetUserByEmail(string email);

    }
}
