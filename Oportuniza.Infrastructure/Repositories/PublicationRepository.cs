using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class PublicationRepository : Repository<Publication>, IPublicationRepository
    {
        private readonly ApplicationDbContext _context;

        public PublicationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Publication>> GetMyPublications(Guid userId)
        {
            var userCompanyIds = await _context.Company
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .ToListAsync();

            var myPublications = await _context.Publication
                .Where(p => p.AuthorUserId == userId || (p.AuthorCompanyId.HasValue && userCompanyIds.Contains(p.AuthorCompanyId.Value)))
                .Where(p => p.IsActive == PublicationAvailable.Enabled)
                .ToListAsync();

            return myPublications;
        }

        public async Task<(IEnumerable<Publication>, int)> GetMyPublicationsPaged(Guid userId, int pageNumber, int pageSize)
        {
            var query = _context.Publication.Where(p => p.AuthorUserId == userId).Where(p => p.IsActive == PublicationAvailable.Enabled);

            var totalCount = await query.CountAsync();

            var publications = await query
                .OrderByDescending(p => p.CreationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (publications, totalCount);
        }
    }
}
