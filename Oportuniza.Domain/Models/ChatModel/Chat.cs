namespace Oportuniza.Domain.Models.ChatModel
{
    public class Chat
    {
        public Guid Id { get; set; }
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
