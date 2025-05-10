namespace Oportuniza.Domain.Models
{
    public class ChatParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ChatId { get; set; } = null!;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
    }
}
