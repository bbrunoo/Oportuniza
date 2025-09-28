using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICompanyEmployeeRepository : IRepository<CompanyEmployee>
    {
        Task<IEnumerable<CompanyEmployee>> GetEmployeesOrderedByRoleAndCreationAsync(Guid companyId);
    }
}
