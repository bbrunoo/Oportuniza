using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyByIdDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
