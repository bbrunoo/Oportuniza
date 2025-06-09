using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company> Add(Company Company)
        {

            if (Company.PasswordHash == null || Company.PasswordSalt == null)
                throw new ArgumentException("Hash e salt da senha são obrigatórios.");

            await _context.Company.AddAsync(Company);
            await _context.SaveChangesAsync();
            return Company;
        }
        public async Task<Company> Delete(Company Company)
        {
            _context.Company.Remove(Company);
            await _context.SaveChangesAsync();
            return Company;
        }
        public async Task<bool> Exist(Guid id)
        {
            return await _context.Company.AnyAsync(c => c.Id == id);
        }
        public async Task<IEnumerable<Company>> Get()
        {
            return await _context.Company.ToListAsync();
        }

        public async Task<IEnumerable<AllCompanyInfoDTO>> GetAllCompanyInfosAsync()
        {
            return await _context.Company
                .Select(u => new AllCompanyInfoDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Desc = u.Desc,
                    imageUrl = u.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<Company?> GetById(Guid id)
        {
            return await _context.Company.FindAsync(id);
        }
        public async Task<CompanyInfoDTO> GetCompanyInfoAsync(Guid id)
        {
            var CompanyInfo = await _context.Company
                .Where(u => u.Id == id)
                .Select(u => new CompanyInfoDTO
                {
                    Name = u.Name,
                    Desc = u.Desc,
                    Email = u.Email,
                })
                .FirstOrDefaultAsync();

            if (CompanyInfo == null) throw new KeyNotFoundException("Company not found");
            return CompanyInfo;
        }
        public async Task<bool> Update(Company Company)
        {
            _context.Company.Update(Company);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
