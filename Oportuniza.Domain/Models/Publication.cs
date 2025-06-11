namespace Oportuniza.Domain.Models
{
    public enum AuthorType
    {
        User, Company
    }
    public class Publication
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string ImageUrl { get; set; }
        public bool Expired { get; set; }
        public Guid AuthorId { get; set; }
        public AuthorType AuthorType { get; set; }
        public virtual User? Author { get; set; }

    }
}
