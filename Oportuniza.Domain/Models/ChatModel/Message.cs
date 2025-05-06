namespace Oportuniza.Domain.Models.ChatModel
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public Chat Chat { get; set; }
    }
}
