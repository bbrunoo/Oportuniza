using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CityState { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Cnpj { get; set; }
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public string ImageUrl { get; set; }
        public string UserRole { get; set; }
        public CompanyAvailable IsActive { get; set; }
        public bool IsDisabled => IsActive == CompanyAvailable.Disabled;
        public bool IsEnabled => IsActive == CompanyAvailable.Active;
    }
}
