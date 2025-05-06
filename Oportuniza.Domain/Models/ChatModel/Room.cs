namespace Oportuniza.Domain.Models.ChatModel
{
    public class Room
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<Member> Members { get; set; } = new List<Member>();
    }
}
