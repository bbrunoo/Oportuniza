namespace Oportuniza.Domain.Conversations
{
    public class ConversationParticipant
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public long UserId { get; set; }
    }
}
