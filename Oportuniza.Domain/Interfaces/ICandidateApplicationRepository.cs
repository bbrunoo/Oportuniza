using Oportuniza.API.Viewmodel;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICandidateApplicationRepository : IRepository<CandidateApplication>
    {
        Task<IEnumerable<CandidateApplication>> GetCandidatesByPublicationAsync(Guid publicationId);
        Task<IEnumerable<CandidateApplication>> GetApplicationsByUserAsync(Guid userId);
        Task<IEnumerable<CandidateApplication>> GetApplicationsLoggedUser(string userId);
        Task<IEnumerable<PublicationWithCandidates>> GetPublicationsWithCandidatesByUserAsync(Guid userId);
        Task<bool> HasAppliedAsync(Guid publicationId, Guid userId);
        Task<object> GetPublicationStatisticsAsync(Guid publicationId);
        Task<CandidateApplication> GetApplicationByPublicationAndUserAsync(Guid publicationId, Guid userId);
    }
}
