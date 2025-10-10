using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Publication;
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

        public async Task<IEnumerable<Publication>> FilterPublicationsAsync(PublicationFilterDto filters)
        {
            var query = _context.Publication
                .Where(p => p.IsActive == PublicationAvailable.Enabled);

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.Title != null && p.Title.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrEmpty(filters.Local))
            {
                var localTerm = filters.Local.Trim().ToLower();
                query = query.Where(p => p.Local != null && p.Local.Trim().ToLower().Contains(localTerm));
            }

            if (filters.Contracts != null && filters.Contracts.Any())
            {
                var contractFilters = filters.Contracts.Select(c => c.Trim().ToLower()).ToList();
                query = query.Where(p => p.Contract != null && contractFilters.Contains(p.Contract.Trim().ToLower()));
            }

            if (filters.Shifts != null && filters.Shifts.Any())
            {
                var shiftFilters = filters.Shifts.Select(s => s.Trim().ToLower()).ToList();
                query = query.Where(p => p.Shift != null && shiftFilters.Contains(p.Shift.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(filters.SalaryRange))
            {
                switch (filters.SalaryRange.ToLower())
                {
                    case "range1":
                        query = query.Where(p => p.Salary != null && p.Salary.Contains("Até a R$1000,00"));
                        break;
                    case "range2":
                        query = query.Where(p => p.Salary != null && p.Salary.Contains("R$1000,00 a R$2000,00"));
                        break;
                    case "range3":
                        query = query.Where(p => p.Salary != null && p.Salary.Contains("Mais de R$2000,00"));
                        break;
                }
            }

            query = query
                .Include(p => p.AuthorUser)
                .Include(p => p.AuthorCompany)
                .OrderByDescending(p => p.CreationDate);
             
            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<Publication> publications, int totalCount)> GetCompanyPublicationsPaged(
                    Guid companyId, int pageNumber, int pageSize)
        {
            var query = _context.Publication
                .Where(p => p.AuthorCompanyId == companyId);

            var totalCount = await query.CountAsync();

            var publications = await query
                .OrderByDescending(p => p.CreationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (publications, totalCount);
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
            var userCompanyIds = await _context.Company
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .ToListAsync();

            var query = _context.Publication
                    .Where(p => p.AuthorUserId == userId || (p.AuthorCompanyId.HasValue && userCompanyIds.Contains(p.AuthorCompanyId.Value)))
                    .Where(p => p.IsActive == PublicationAvailable.Enabled);

            var totalCount = await query.CountAsync();

            var publications = await query
                .OrderByDescending(p => p.CreationDate)
                .Include(p => p.AuthorUser)
                .Include(p => p.AuthorCompany)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return (publications, totalCount);
        }

        public async Task<(IEnumerable<Publication>, int)> GetMyPublicationsPaged(Guid userId, int pageNumber, int pageSize, bool onlyPersonal)
        {
            if (!onlyPersonal)
            {
                return await GetMyPublicationsPaged(userId, pageNumber, pageSize);
            }

            var query = _context.Publication
                .Where(p => p.AuthorUserId == userId && p.IsActive == PublicationAvailable.Enabled);

            var totalCount = await query.CountAsync();

            var publications = await query
                .OrderByDescending(p => p.CreationDate)
                .Include(p => p.AuthorUser)
                .Include(p => p.AuthorCompany)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (publications, totalCount);
        }
    }
}
