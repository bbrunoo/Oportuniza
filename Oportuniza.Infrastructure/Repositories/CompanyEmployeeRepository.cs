using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CompanyEmployeeRepository : Repository<CompanyEmployee>, ICompanyEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyEmployeeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<CompanyEmployee?> GetByUserAndCompanyAsync(Guid userId, Guid companyId)
        {
            return await _context.CompanyEmployee
                .Include(e => e.CompanyRole)
                .FirstOrDefaultAsync(e =>
                    e.UserId == userId &&
                    e.CompanyId == companyId &&
                    e.IsActive == CompanyEmployeeStatus.Active);
        }

        public async Task<IEnumerable<CompanyEmployee>> GetByUserIdAsync(Guid userId)
        {
            return await _context.CompanyEmployee
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Company>> GetCompaniesByEmployeeAsync(Guid userId)
        {
            return await _context.CompanyEmployee
                .Where(ce => ce.UserId == userId && ce.IsActive == CompanyEmployeeStatus.Active)
                .Select(ce => ce.Company)
                .ToListAsync();
        }

        public async Task<CompanyEmployee?> GetEmployeeByUserIdAndCompanyIdAsync(Guid userId, Guid companyId)
        {
            return await _context.CompanyEmployee
                                 .FirstOrDefaultAsync(ce => ce.UserId == userId && ce.CompanyId == companyId);
        }

        public async Task<IEnumerable<CompanyEmployee>> GetEmployeesOrderedByRoleAndCreationAsync(Guid companyId)
        {
            return await _context.CompanyEmployee
                .Where(ce => ce.CompanyId == companyId)
                .Include(ce => ce.User)
                .Include(ce => ce.CompanyRole)
                .OrderByDescending(ce => ce.CompanyRole.Name == "Owner")
                .ThenBy(ce => ce.Id)
                .ToListAsync();
        }

        public async Task<CompanyEmployee> GetUserCompanyRoleAsync(Guid userId, Guid companyId)
        {
            return await _context.CompanyEmployee
                .AsNoTracking()
                .FirstOrDefaultAsync(ce => ce.UserId == userId && ce.CompanyId == companyId);
        }

        public async Task<bool> IsUserEmployeeInAnyCompanyAsync(Guid userId)
        {
            return await _context.CompanyEmployee
                .AsNoTracking() 
                .AnyAsync(ce => ce.UserId == userId);
        }
    }
}
