namespace Oportuniza.Domain.Models.ChatModel
{
    public class Member
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid RoomId { get; set; }
        public DateTime JoinedOn { get; set; }
    }
}
