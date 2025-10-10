namespace Oportuniza.Domain.DTOs.Employee
{
    public class UserSearchResultDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string? Name { get; set; }
        public string ImageUrl { get; set; }
    }

}
