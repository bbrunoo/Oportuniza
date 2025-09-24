using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Company;
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
                             .Include(c => c.Employees)
                             .ThenInclude(e => e.User)
                             .AsNoTracking()
                             .ToListAsync();
        }

        public async Task<List<Company>> GetByUserIdAsync(Guid userId)

        {

            return await _context.Company

                .Where(c => c.UserId == userId)

                .ToListAsync();

        }

        public async Task<List<Company>> GetByUserIdAsyncPaginated(Guid userId, int pageNumber, int pageSize)
        {
            var skipAmount = (pageNumber - 1) * pageSize;

            return await _context.Company
                .Where(c => c.UserId == userId)
                .Skip(skipAmount)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId)
        {
            bool hasAccess = await _context.CompanyEmployee
                        .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);

            return hasAccess;
        }
    }
}
