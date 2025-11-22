using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ILoginAttemptRepository
    {
        Task<LoginAttempt?> GetByIpAsync(string ip);
        Task AddAsync(LoginAttempt attempt);
        Task UpdateAsync(LoginAttempt attempt);
    }

}
