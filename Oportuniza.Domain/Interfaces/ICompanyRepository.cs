using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<List<Company>> GetByUserIdAsync(Guid userId);
        Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId);
        Task<IEnumerable<Company>> GetAllWithEmployeesAndUsersAsync();
    }
}
