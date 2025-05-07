namespace Oportuniza.Domain.Conversations
{
    public class Participant
    {
        public long UserId { get; set; }
        public Guid ConversationId { get; set; }
    }
}
