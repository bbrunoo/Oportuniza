using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<List<Company>> GetByUserIdAsyncPaginated(Guid userId, int pageNumber, int pageSize);
        Task<List<Company>> GetByUserIdAsync(Guid userId);
        Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId);
        Task<IEnumerable<Company>> GetAllWithEmployeesAndUsersAsync();
        Task<Company> GetByIdWithEmployeesAndUsersAsync(Guid companyId);
        Task<(List<Company> Companies, int TotalCount)> GetUserCompaniesPaginatedAsync(
            Guid userId,
            int pageNumber,
            int pageSize
        );
        Task<List<Company>> GetAllByUserOrEmployeeAsync(Guid userId);
        Task<bool> UserOwnsCompanyAsync(Guid userId, Guid companyId);
    }
}
