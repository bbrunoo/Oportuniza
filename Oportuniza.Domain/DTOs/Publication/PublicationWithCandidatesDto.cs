using Oportuniza.Domain.DTOs.Candidates;

namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationWithCandidatesDto
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
        public List<CandidateDto> Candidates { get; set; }
    }
}
