using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.Candidates
{
    public class CandidatesDTO
    {
        public Guid Id { get; set; }
        public Guid PublicationId { get; set; }
        public string PublicationTitle { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public CandidateApplicationStatus Status { get; set; }
    }
}
