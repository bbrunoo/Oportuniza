using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Company>> GetAllWithEmployeesAndUsersAsync()
        {
            return await _context.Company
                            .Where(c => c.IsActive == CompanyAvailable.Active)
                             .Include(c => c.Employees)
                             .ThenInclude(e => e.User)
                             .AsNoTracking()
                             .ToListAsync();
        }

        public async Task<Company> GetByIdWithEmployeesAndUsersAsync(Guid companyId)
        {
            return await _context.Company
                    .Where(c => c.Id == companyId)
                    .Include(c=>c.Manager)
                    .Include(c => c.Employees)
                        .ThenInclude(ce => ce.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
        }

        public async Task<List<Company>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Company
               .Where(c => c.IsActive == CompanyAvailable.Active)
               .Where(c => c.UserId == userId)
               .ToListAsync();
        }

        public async Task<List<Company>> GetByUserIdAsyncPaginated(Guid userId, int pageNumber, int pageSize)
        {
            var skipAmount = (pageNumber - 1) * pageSize;

            return await _context.Company
                .Where(c => c.IsActive == CompanyAvailable.Active)
                .Where(c => c.UserId == userId)
                .Skip(skipAmount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Company>> GetAllByUserOrEmployeeAsync(Guid userId)
        {
            return await _context.Company
                .Include(c => c.Employees)
                .Where(c => c.IsActive == CompanyAvailable.Active &&
                           (c.UserId == userId || c.Employees.Any(e => e.UserId == userId)))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<(List<Company> Companies, int TotalCount)> GetUserCompaniesPaginatedAsync(Guid userId, int pageNumber, int pageSize)
        {
            var employedCompanyIds = await _context.CompanyEmployee
                .Where(ce => ce.UserId == userId)
                .Select(ce => ce.CompanyId)
                .ToListAsync();

            var allUserCompaniesQuery = _context.Company
                .Where(c => c.UserId == userId || employedCompanyIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .AsQueryable();

            var totalCount = await allUserCompaniesQuery.CountAsync();

            var companies = await allUserCompaniesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (companies, totalCount);
        }

        public async Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId)
        {
            return await _context.Company.AnyAsync(c =>
                c.Id == companyId &&
                c.IsActive == CompanyAvailable.Active &&
                (
                    c.UserId == userId ||
                    c.Employees.Any(e =>
                        e.UserId == userId &&
                        e.IsActive == CompanyEmployeeStatus.Active
                    )
                ));
        }

        public async Task<bool> UserOwnsCompanyAsync(Guid userId, Guid companyId)
        {
            return await _context.Company
                .AnyAsync(c => c.Id == companyId && c.UserId == userId);
        }

        public async Task<List<Company>> GetCompaniesByUserIdAsync(Guid userId)
        {
            return await _context.CompanyEmployee
                .Where(e => e.UserId == userId && e.IsActive == CompanyEmployeeStatus.Active)
                .Include(e => e.Company)
                .Select(e => e.Company)
                .ToListAsync();
        }
    }
}
