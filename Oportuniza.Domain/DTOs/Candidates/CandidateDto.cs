namespace Oportuniza.Domain.DTOs.Candidates
{
    public class CandidateDto
    {
        public Guid CandidateId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserImage { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
    }
}
