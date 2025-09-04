using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.User
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ImageUrl { get; set; }
        public List<UserAreaOfInterest> AreaOfInterest { get;set; }
        public string Local { get; set; }
        public bool IsProfileCompleted{ get; set; }
    }
}
