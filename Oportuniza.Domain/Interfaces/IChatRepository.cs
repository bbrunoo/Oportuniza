using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IChatRepository
    {
        Task SaveMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesForChatAsync(string chatId);
        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
    }
}
