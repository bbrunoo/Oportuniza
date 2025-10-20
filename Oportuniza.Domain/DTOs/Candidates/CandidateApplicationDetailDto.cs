namespace Oportuniza.Domain.DTOs.Candidates
{
    public class CandidateApplicationDetailDto
    {
        public Guid PublicationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Resumee { get; set; }
        public string AuthorImage { get; set; }
        public string ImageUrl { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string? ProfileImage { get; set; }
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }
        public int TotalApplicationsForThisJob { get; set; }
    }

}
