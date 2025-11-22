using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.DTOs.Candidates
{
    public class PutCandidatesDTO
    {
        public Guid Id { get; set; }
        public CandidateApplicationStatus Status { get; set; }
    }
}
