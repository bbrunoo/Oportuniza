using Oportuniza.Domain.DTOs.Candidates.CandidateExtra;
using Oportuniza.Domain.DTOs.Publication;

namespace Oportuniza.Domain.DTOs.Extra
{
    public class UserApplicationDto
    {
        public Guid Id { get; set; }
        public PublicationDto Publication { get; set; }
        public CandidateExtraDTO? CandidateExtra { get; set; }

    }
}
