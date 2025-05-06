namespace Oportuniza.Domain.Conversations
{
    public class Conversations
    {
        public Guid Id { get; private set; }
        public long? RoomId { get; private set; }
        public List<long> Participants { get; private set; }
        public bool IsGroup { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public DateTime UpdatedOn { get; private set; }
    }
}
