using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class PublicationRepository : Repository<Publication>, IPublicationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Publication> _dbSet;

        public PublicationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Publication>();
        }
    }
}
