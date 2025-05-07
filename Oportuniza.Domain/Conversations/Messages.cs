namespace Oportuniza.Domain.Conversations
{
    public class Message
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public Guid ConversationId { get; private set; }
        public long SenderId { get; private set; }
        public string SenderName { get; private set; }
        public string Content { get; private set; }
        public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;
    }
}
