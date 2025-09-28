using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.Models
{
    public class CompanyEmployee
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid CompanyId { get; set; }
        public CompanyEmployeeStatus IsActive { get; set; } = CompanyEmployeeStatus.Active;
        public virtual Company Company { get; set; }
        public string Roles { get; set; }
        public bool CanPostJobs { get; set; }
    }
}
