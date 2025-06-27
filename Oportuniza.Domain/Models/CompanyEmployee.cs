namespace Oportuniza.Domain.Models
{
    public class CompanyEmployee
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid AzureUserId { get; set; }
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public string Roles { get; set; }
        public bool CanPostJobs { get; set; }
    }

}
