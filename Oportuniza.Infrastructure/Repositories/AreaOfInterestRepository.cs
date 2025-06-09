using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class AreaOfInterestRepository : Repository<AreaOfInterest>, IAreaOfInterest
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<AreaOfInterest> _dbSet;

        public AreaOfInterestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<AreaOfInterest>();
        }

        public async Task DeleteAllAsync()
        {
            _dbSet.RemoveRange(_dbSet);
            await _context.SaveChangesAsync();
        }

        public async Task<AreaOfInterest?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.InterestArea.ToLower() == name.ToLower());
        }
    }
}
