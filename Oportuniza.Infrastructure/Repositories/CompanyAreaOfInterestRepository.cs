using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CompanyAreaOfInterestRepository : Repository<CompanyAreaOfInterest>, ICompanyAreaOfInterestRepository
    {
        public CompanyAreaOfInterestRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
