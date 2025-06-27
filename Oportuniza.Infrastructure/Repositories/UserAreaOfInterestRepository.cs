using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class UserAreaOfInterestRepository : Repository<UserAreaOfInterest>, IUserAreaOfInterestRepository
    {
        public UserAreaOfInterestRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
