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
                .Include(ce => ce.Company)
                .Include(ce => ce.User)
                .FirstOrDefaultAsync(ce => ce.UserId == userId
                                        && ce.CompanyId == companyId
                                        && ce.IsActive == CompanyEmployeeStatus.Active);
        }

        public async Task<List<Company>> GetCompaniesByEmployeeAsync(Guid userId)
        {
            return await _context.CompanyEmployee
                .Where(ce => ce.UserId == userId && ce.IsActive == CompanyEmployeeStatus.Active)
                .Select(ce => ce.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompanyEmployee>> GetEmployeesOrderedByRoleAndCreationAsync(Guid companyId)
        {
            return await _context.CompanyEmployee
                .Where(ce => ce.CompanyId == companyId && ce.IsActive == CompanyEmployeeStatus.Active)
                .Include(ce => ce.User)
                .OrderByDescending(ce => ce.Roles == "Owner")
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
