namespace Oportuniza.Domain.Conversations
{
    public class Messages
    {
        public string Id { get; private set; }
        public string ConversationId { get; private set; }
        public long SenderId { get; private set; }
        public string SenderName { get; private set; }
        public string Content { get; private set; }
        public DateTime CreatedOn { get; private set; }
    }
}
