using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICandidateExtraRepository
    {
        Task<CandidateExtra> AddAsync(CandidateExtra entity);
        Task<CandidateExtra?> GetByCandidateApplicationIdAsync(Guid applicationId);
    }

}
