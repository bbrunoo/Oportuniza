namespace Oportuniza.Domain.DTOs.User
{
    public class CompleteProfileDTO
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsACompany { get; set; }
        public string? Interests { get; set; }
        public string ImageUrl { get; set; }
    }
}
