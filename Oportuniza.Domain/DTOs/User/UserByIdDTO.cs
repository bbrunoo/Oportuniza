namespace Oportuniza.Domain.DTOs.User
{
    public class UserByIdDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool isACompany { get; set; }
    }
}
