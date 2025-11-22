using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CandidateExtraRepository : ICandidateExtraRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateExtraRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateExtra> AddAsync(CandidateExtra entity)
        {
            _context.CandidateExtra.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CandidateExtra?> GetByCandidateApplicationIdAsync(Guid applicationId)
        {
            return await _context.CandidateExtra
                .FirstOrDefaultAsync(x => x.CandidateApplicationId == applicationId);
        }
    }

}
