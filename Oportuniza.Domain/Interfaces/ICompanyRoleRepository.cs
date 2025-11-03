using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICompanyRoleRepository : IRepository<CompanyRole>
    {
        Task<CompanyRole?> GetRoleByNameAsync(string name);
        Task<string?> GetRoleNameByIdAsync(Guid id);

    }
}
