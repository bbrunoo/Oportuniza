namespace Oportuniza.Domain.DTOs.Chat
{
    public class ChatHistoryDto
    {
        public string ChatId { get; set; } = null!;
        public List<string> Participants { get; set; } = new();
        public string? LastMessage { get; set; }
        public DateTime? LastMessageDate { get; set; }
    }
}
