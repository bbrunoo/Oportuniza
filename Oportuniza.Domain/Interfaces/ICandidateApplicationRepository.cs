using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICandidateApplicationRepository : IRepository<CandidateApplication>
    {
        Task<IEnumerable<CandidateApplication>> GetCandidatesByPublicationAsync(Guid publicationId);
        Task<IEnumerable<CandidateApplication>> GetApplicationsByUserAsync(Guid userId);
        //Task<IEnumerable<Publication>> GetPublicationsWithCandidatesByUserAsync(Guid userId);
        Task<bool> HasAppliedAsync(Guid publicationId, Guid userId);
        Task<object> GetPublicationStatisticsAsync(Guid publicationId);
        Task<CandidateApplication> GetApplicationByPublicationAndUserAsync(Guid publicationId, Guid userId);
        //Task<IEnumerable<CandidateApplication>> GetApplicationsLoggedUser(Guid userId);
        //Task<IEnumerable<Publication>> GetApplicationsLoggedUser(Guid userId);
        Task<IEnumerable<CandidateApplication>> GetApplicationsLoggedUser(Guid userId);
        Task<List<PublicationWithCandidatesDto>> GetPublicationsWithCandidatesByAuthorAsync(Guid publicationAuthorId);
    }
}
