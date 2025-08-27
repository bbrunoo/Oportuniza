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
    }
}
