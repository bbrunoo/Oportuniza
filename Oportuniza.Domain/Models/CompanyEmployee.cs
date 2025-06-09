namespace Oportuniza.Domain.Models
{
    public class CompanyEmployee
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        public bool CanPostJobs { get; set; }
    }

}
