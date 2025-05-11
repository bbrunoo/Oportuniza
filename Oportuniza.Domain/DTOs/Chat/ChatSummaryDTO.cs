namespace Oportuniza.Domain.DTOs.Chat
{
    public class ChatSummaryDto
    {
        public string ChatId { get; set; }
        public Guid TargetUserId { get; set; }
        public string TargetUserName { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageDate { get; set; }
    }
}
