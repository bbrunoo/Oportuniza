using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> Exist(Guid id);
        Task<UserInfoDTO> GetUserInfoAsync(Guid id);
        Task<IEnumerable<AllUsersInfoDTO>> GetAllUserInfosAsync();
        Task<User?> GetByIdWithInterests(Guid id);
        Task<User?> GetByIdentityProviderIdAsync(string identityProviderId, string identityProviderType);
    }
}
