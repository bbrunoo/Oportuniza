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
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.AuthorUser)
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.AuthorCompany)
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.CreatedByUser)
                .Include(ca => ca.User)
                .Where(ca => ca.UserId == userId)
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
                    .ThenInclude(p => p.AuthorCompany)
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.CreatedByUser)
                .Include(ca => ca.User)
                .Include(ca => ca.CandidateExtra)
                .Where(ca => ca.Publication.AuthorUser != null && ca.Publication.AuthorUserId == publicationAuthorId)
                .GroupBy(ca => ca.Publication)
                .Select(group => new PublicationWithCandidatesDto
                {
                    PublicationId = group.Key.Id,
                    Title = group.Key.Title,
                    Description = group.Key.Description,
                    Resumee = group.Key.Resumee,
                    ImageUrl = group.Key.ImageUrl,
                    CreationDate = group.Key.CreationDate,
                    AuthorId = group.Key.AuthorUser != null
                        ? group.Key.AuthorUser.Id
                        : group.Key.AuthorCompany != null
                            ? group.Key.AuthorCompany.Id
                            : group.Key.CreatedByUser.Id,
                    AuthorName = group.Key.AuthorUser != null
                        ? group.Key.AuthorUser.Name
                        : group.Key.AuthorCompany != null
                            ? group.Key.AuthorCompany.Name
                            : group.Key.CreatedByUser.Name,
                    AuthorImage = group.Key.AuthorUser != null
                        ? group.Key.AuthorUser.ImageUrl
                        : group.Key.AuthorCompany != null
                            ? group.Key.AuthorCompany.ImageUrl
                            : group.Key.CreatedByUser.ImageUrl,

                    Candidates = group.Select(ca => new CandidateDto
                    {
                        CandidateId = ca.Id,
                        UserId = ca.UserId,
                        UserName = ca.User.Name,
                        Email = ca.User.Email,
                        UserImage = ca.User.ImageUrl,
                        ApplicationDate = ca.ApplicationDate,
                        Status = ca.Status.ToString(),

                        Observation = ca.CandidateExtra != null ? ca.CandidateExtra.Observation : null,
                        ResumeUrl = ca.CandidateExtra != null ? ca.CandidateExtra.ResumeUrl : null
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CandidateApplicationDetailDto>> GetApplicationsByCompanyAsync(Guid companyId)
        {
            return await _context.CandidateApplication
                .Where(ca => ca.Publication.AuthorCompanyId == companyId)

                .Include(ca => ca.User)
                .Include(ca => ca.Publication)
                    .ThenInclude(p => p.AuthorCompany)
                .Include(ca => ca.CandidateExtra)

                .OrderByDescending(ca => ca.ApplicationDate)
                .Select(ca => new CandidateApplicationDetailDto
                {
                    ApplicationId = ca.Id,
                    Status = ca.Status.ToString(),
                    CreationDate = ca.Publication.CreationDate,

                    UserId = ca.User.Id,
                    UserName = ca.User.Name,
                    UserEmail = ca.User.Email,
                    ProfileImage = ca.User.ImageUrl,

                    PublicationId = ca.Publication.Id,
                    Title = ca.Publication.Title,
                    Description = ca.Publication.Description,
                    Resumee = ca.Publication.Resumee,
                    ImageUrl = ca.Publication.ImageUrl,
                    AuthorImage = ca.Publication.AuthorCompany.ImageUrl,
                    AuthorId = ca.Publication.AuthorCompany.Id,
                    AuthorName = ca.Publication.AuthorCompany.Name,

                    ResumeUrl = ca.CandidateExtra != null ? ca.CandidateExtra.ResumeUrl : null,
                    Observation = ca.CandidateExtra != null ? ca.CandidateExtra.Observation : null,

                    TotalApplicationsForThisJob = ca.Publication.CandidateApplication.Count()
                })
                .ToListAsync();
        }
    }
}
