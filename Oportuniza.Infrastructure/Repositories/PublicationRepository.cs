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
                .Where(p => p.IsActive == PublicationAvailable.Enabled)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var searchTerm = $"%{filters.SearchTerm.Trim()}%";
                query = query.Where(p =>
                    (p.Resumee != null && EF.Functions.Like(EF.Functions.Collate(p.Resumee, "Latin1_General_CI_AI"), searchTerm)) ||
                    (p.Title != null && EF.Functions.Like(EF.Functions.Collate(p.Title, "Latin1_General_CI_AI"), searchTerm))
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
                        query = query.Where(p => p.Salary != null && p.Salary.Contains("Até R$1.000,00"));
                        break;
                    case "range2":
                        query = query.Where(p => p.Salary != null && p.Salary.Contains("R$1.000,00 a R$2.000,00"));
                        break;
                    case "range3":
                        query = query.Where(p => p.Salary != null && p.Salary.Contains("Acima de R$2.000,00"));
                        break;
                }
            }

            var list = await query
                .Include(p => p.AuthorUser)
                .Include(p => p.AuthorCompany)
                .OrderByDescending(p => p.CreationDate)
                .ToListAsync();

            if (filters.Latitude.HasValue && filters.Longitude.HasValue && filters.RadiusKm.HasValue && filters.RadiusKm > 0)
            {
                double lat = filters.Latitude.Value;
                double lng = filters.Longitude.Value;
                double radius = filters.RadiusKm.Value;

                list = list
                    .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                    .Where(p =>
                    {
                        var d = DistanceInKm(lat, lng, p.Latitude.Value, p.Longitude.Value);
                        return d <= radius;
                    })
                    .ToList();
            }

            return list;
        }
        private static double DistanceInKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) *
                Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
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

        public async Task UpdateRangeAsync(IEnumerable<Publication> publications)
        {
            _context.Publication.UpdateRange(publications);
            await _context.SaveChangesAsync();
        }
    }
}
