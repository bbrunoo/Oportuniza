using Oportuniza.Domain.Enums;
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
        public string Salary { get; set; }
        public bool Expired { get; set; }
        public string Contract { get; set; }
        public string Shift { get; set; }
        public string Local { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorType { get; set; }
        public string AuthorImageUrl { get; set; }
        public PublicationStatus Status { get; set; }
    }
}
