using Oportuniza.Domain.DTOs.User;

namespace Oportuniza.Domain.DTOs
{
    public class CompanyUserDTO : UserDTO
    {
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Role { get; set; }
    }
}
