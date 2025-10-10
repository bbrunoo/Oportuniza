using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CompanyRoleRepository : Repository<CompanyRole>, ICompanyRoleRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<CompanyRole?> GetRoleByNameAsync(string name)
        {
            return await _context.CompanyRole
                                 .FirstOrDefaultAsync(r => r.Name == name);
        }
    }
}
