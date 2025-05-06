namespace Oportuniza.API.Controllers.Conversations
{
    public class CreateConversationsRequest
    {
        public Guid RoomId { get; set; }
        public List<long> participants { get; set; }
    }
}
