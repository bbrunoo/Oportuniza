using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CandidateApplicationRepository : Repository<CandidateApplication>, ICandidateApplicationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Publication> _dbSet;

        public CandidateApplicationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Publication>();
        }

        public async Task<IEnumerable<CandidateApplication>> GetCandidatesByPublicationAsync(Guid publicationId)
        {
            return await _context.CandidateApplication
                .Include(ca => ca.User)
                .Include(ca => ca.Publication)
                .Where(ca => ca.PublicationId == publicationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CandidateApplication>> GetApplicationsByUserAsync(Guid userId)
        {
            return await _context.CandidateApplication
                .Include(ca => ca.User)
                .Include(ca => ca.Publication)
                .Where(ca => ca.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> HasAppliedAsync(Guid publicationId, Guid userId)
        {
            return await _context.CandidateApplication
                .AnyAsync(ca => ca.PublicationId == publicationId && ca.UserId == userId);
        }

        public async Task<object> GetPublicationStatisticsAsync(Guid publicationId)
        {
            var apps = await _context.CandidateApplication
                .Where(ca => ca.PublicationId == publicationId)
                .ToListAsync();

            return new
            {
                total = apps.Count,
                pending = apps.Count(ca => ca.Status == CandidateApplicationStatus.Pending),
                approved = apps.Count(ca => ca.Status == CandidateApplicationStatus.Approved),
                rejected = apps.Count(ca => ca.Status == CandidateApplicationStatus.Rejected)
            };
        }
    }
}
