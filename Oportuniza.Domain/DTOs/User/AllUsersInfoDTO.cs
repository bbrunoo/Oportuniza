namespace Oportuniza.Domain.DTOs.User
{
    public class AllUsersInfoDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool isACompany { get; set; }
        public string imageUrl { get; set; }
    }
}
