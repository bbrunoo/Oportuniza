using Microsoft.AspNetCore.Http;

namespace Oportuniza.Domain.DTOs.Candidates.CandidateExtra
{
    public class CreateCandidateExtraDTO
    {
        public string? Observation { get; set; }
        public IFormFile? Resume { get; set; }
    }
}
