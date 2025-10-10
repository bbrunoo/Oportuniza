using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICompanyEmployeeRepository : IRepository<CompanyEmployee>
    {
        Task<IEnumerable<CompanyEmployee>> GetEmployeesOrderedByRoleAndCreationAsync(Guid companyId);
        Task<CompanyEmployee?> GetByUserAndCompanyAsync(Guid userId, Guid companyId);
        Task<CompanyEmployee?> GetEmployeeByUserIdAndCompanyIdAsync(Guid userId, Guid companyId);
        Task<IEnumerable<CompanyEmployee>> GetByUserIdAsync(Guid userId);
    }
}
