using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool Expired { get; set; }
        public Guid AuthorId { get; set; }
        public int AuthorType { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImageUrl { get; set; }
    }
}
