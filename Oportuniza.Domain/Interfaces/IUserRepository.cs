
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> Add(User user);
        Task<IEnumerable<User>> Get();
        Task<User?> GetById(Guid id);
        Task<bool> Exist(Guid id);
        Task<User> Delete(User user);
        Task<UserInfoDTO> GetUserInfoAsync(Guid id);
        Task<bool> Update(User user);
        Task<IEnumerable<AllUsersInfoDTO>> GetAllUserInfosAsync();
    }
}
