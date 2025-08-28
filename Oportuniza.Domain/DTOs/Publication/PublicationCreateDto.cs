using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Salary { get; set; }
        public string Contract { get; set; }
        public string Shift { get; set; }
        public string Local { get; set; }
        public DateTime ExpirationDate { get; set; }
        public Guid? PostAsCompanyId { get; set; }
    }
}
