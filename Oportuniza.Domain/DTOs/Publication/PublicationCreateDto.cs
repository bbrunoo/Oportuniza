using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public Guid AuthorId { get; set; }
    }
}
