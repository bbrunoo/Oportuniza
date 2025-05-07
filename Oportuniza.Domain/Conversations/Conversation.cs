namespace Oportuniza.Domain.Conversations
{
    public class Conversation
    {
        public Guid Id { get; private set; }
        public long? RoomId { get; private set; }
        public List<ConversationParticipant> Participants { get; private set; } = new();
        public bool IsGroup => RoomId.HasValue;
        public DateTime CreatedOn { get; private set; }
        public DateTime UpdatedOn { get; private set; }
    }
}
