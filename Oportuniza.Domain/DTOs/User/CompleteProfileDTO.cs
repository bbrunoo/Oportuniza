namespace Oportuniza.Domain.DTOs.User
{
    public class CompleteProfileDTO
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string? Interests { get; set; }
        public string ImageUrl { get; set; }
        public List<Guid> AreaOfInterestIds { get; set; }

    }
}
