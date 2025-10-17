namespace Oportuniza.Domain.Models
{
    public class CandidateExtra
    {
        public Guid Id { get; set; }

        public Guid CandidateApplicationId { get; set; }
        public CandidateApplication CandidateApplication { get; set; }

        public string? Observation { get; set; }
        public string? ResumeUrl { get; set; }
    }
}
