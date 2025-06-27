using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.User
{
    public class UserByIdDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
