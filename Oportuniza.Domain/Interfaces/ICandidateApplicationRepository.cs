using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICandidateApplicationRepository : IRepository<CandidateApplication>
    {
        Task<IEnumerable<CandidateApplication>> GetCandidatesByPublicationAsync(Guid publicationId);
        Task<IEnumerable<CandidateApplication>> GetApplicationsByUserAsync(Guid userId);
        Task<bool> HasAppliedAsync(Guid publicationId, Guid userId);
        Task<object> GetPublicationStatisticsAsync(Guid publicationId);
    }
}
