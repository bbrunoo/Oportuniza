using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CityRepository : Repository<City>, ICityRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<City> _dbSet;

        public CityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<City>();
        }

        public async Task DeleteAllAsync()
        {
            _dbSet.RemoveRange(_dbSet);
            await _context.SaveChangesAsync();
        }

        public async Task<City?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<City>> GetByUfAsync(string uf)
        {
            return await _dbSet
                .Where(c => c.Uf.ToLower() == uf.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<City>> GetCitiesAsync(string? uf, string? name)
        {
            var query = _context.City.AsQueryable();

            if (!string.IsNullOrWhiteSpace(uf))
                query = query.Where(c => c.Uf.ToLower() == uf.ToLower());

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.ToLower().Contains(name.ToLower()));

            return await query.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
