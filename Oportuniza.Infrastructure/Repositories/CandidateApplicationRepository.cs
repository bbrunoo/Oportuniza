using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Candidates;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CandidateApplicationRepository : Repository<CandidateApplication>, ICandidateApplicationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CandidateApplicationRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<CandidateApplication> GetApplicationByPublicationAndUserAsync(Guid publicationId, Guid userId)
        {
            return await _context.CandidateApplication
                                 .FirstOrDefaultAsync(ca => ca.PublicationId == publicationId && ca.UserId == userId);
        }

        public async Task<IEnumerable<CandidateApplication>> GetApplicationsLoggedUser(Guid userId)
        {
            return await _context.CandidateApplication
              .Where(ca => ca.UserId == userId)
              .Include(ca => ca.Publication)
              .ThenInclude(p => p.AuthorUser)
              .ToListAsync();
        }

        public async Task<IEnumerable<Publication>> GetPublicationsWithCandidatesByUserAsync(Guid userId)
        {
            var publications = await _context.Publication
                .Include(p => p.CandidateApplication)
                .ThenInclude(ca => ca.User)
                .Where(p => p.AuthorUserId == userId && p.CandidateApplication.Any())
                .ToListAsync();

            return publications;
        }

        public async Task<List<PublicationWithCandidatesDto>> GetPublicationsWithCandidatesByAuthorAsync(Guid publicationAuthorId)
        {
            return await _context.CandidateApplication
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.AuthorUser)
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.CreatedByUser)
                .Include(ca => ca.User)
                .Where(ca => ca.PublicationAuthorId == publicationAuthorId)
                .GroupBy(ca => ca.Publication)
                .Select(group => new PublicationWithCandidatesDto
                {
                    PublicationId = group.Key.Id,
                    Title = group.Key.Title,
                    Description = group.Key.Description,
                    Resumee = group.Key.Resumee,
                    ImageUrl = group.Key.ImageUrl,
                    AuthorImage = group.Key.AuthorUser != null
                        ? group.Key.AuthorUser.ImageUrl
                        : group.Key.CreatedByUser.ImageUrl,
                    CreationDate = group.Key.CreationDate,
                    Candidates = group.Select(ca => new CandidateDto
                    {
                        CandidateId = ca.Id,
                        UserId = ca.UserId,
                        UserName = ca.User.Name,
                        Email = ca.User.Email,
                        UserImage = ca.User.ImageUrl,
                        ApplicationDate = ca.ApplicationDate,
                        Status = ca.Status.ToString()
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
