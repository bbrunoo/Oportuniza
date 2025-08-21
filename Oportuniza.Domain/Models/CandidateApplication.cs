namespace Oportuniza.Domain.Models
{
    public enum CandidateApplicationStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class CandidateApplication
    {
        public Guid Id { get; set; }

        public Guid PublicationId { get; set; }
        public virtual Publication Publication { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public CandidateApplicationStatus Status { get; set; } = CandidateApplicationStatus.Pending;
    }
}
