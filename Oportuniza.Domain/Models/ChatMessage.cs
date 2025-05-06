namespace Oportuniza.Domain.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ChatId { get; set; } = null!;
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
