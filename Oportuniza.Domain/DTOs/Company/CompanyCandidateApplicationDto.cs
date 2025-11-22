using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyCandidateApplicationDto
    {
        public Guid Id { get; set; }
        public DateTime ApplicationDate { get; set; }
        public CandidateApplicationStatus Status { get; set; }
        public string UserIdKeycloak { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserResumee { get; set; }
        public string UserImageUrl { get; set; }
        public PublicationDto Publication { get; set; }
    }
}
