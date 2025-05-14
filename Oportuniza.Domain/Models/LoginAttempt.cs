namespace Oportuniza.Domain.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string IPAddress { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

}
