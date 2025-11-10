using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.Models
{
    public class Publication
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Resumee { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string ImageUrl { get; set; }
        public string Salary { get; set; }
        public string Shift { get; set; }
        public string Contract { get; set; }
        public string Local { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool Expired { get; set; }
        public Guid CreatedByUserId { get; set; } 
        public virtual User CreatedByUser { get; set; }
        public Guid? AuthorUserId { get; set; }
        public virtual User? AuthorUser { get; set; }
        public Guid? AuthorCompanyId { get; set; }
        public virtual Company? AuthorCompany { get; set; }
        public PublicationStatus Status { get; set; } = PublicationStatus.Pending;
        public ICollection<CandidateApplication> CandidateApplication { get; set; }
        public PublicationAvailable IsActive { get; set; } = PublicationAvailable.Enabled;
    }
}
