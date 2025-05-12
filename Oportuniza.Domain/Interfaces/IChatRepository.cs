using Oportuniza.Domain.DTOs.Chat;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IChatRepository
    {
        Task SaveMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesForChatAsync(string chatId);
        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
        Task<List<ChatSummaryDto>> GetUserChatsAsync(Guid userId);
        Task<string> EnsureChatAndParticipantsAsync(Guid userAId, string userAName, Guid userBId, string userBName);
        Task<bool> ChatExistsAsync(Guid chatId);
        Task CreateChatAsync(PrivateChat chat);
        Task<string> GetUserNameById(Guid userId);
    }
}
