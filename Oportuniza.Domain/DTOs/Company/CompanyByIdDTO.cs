using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyByIdDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CityState { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Cnpj { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
