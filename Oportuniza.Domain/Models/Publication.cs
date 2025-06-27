using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.Models
{
    public class Publication
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string ImageUrl { get; set; }
        public string Salary { get; set; }
        public bool Expired { get; set; }
        public Guid CreatedByUserId { get; set; } 
        public virtual User CreatedByUser { get; set; }
        public Guid? AuthorUserId { get; set; }
        public virtual User? AuthorUser { get; set; }
        public Guid? AuthorCompanyId { get; set; }
        public virtual Company? AuthorCompany { get; set; }

    }
}
