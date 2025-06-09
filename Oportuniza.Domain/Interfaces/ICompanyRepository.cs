
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company> Add(Company Company);
        Task<IEnumerable<Company>> Get();
        Task<Company?> GetById(Guid id);
        Task<bool> Exist(Guid id);
        Task<Company> Delete(Company Company);
        Task<CompanyInfoDTO> GetCompanyInfoAsync(Guid id);
        Task<bool> Update(Company Company);
        Task<IEnumerable<AllCompanyInfoDTO>> GetAllCompanyInfosAsync();
    }
}
