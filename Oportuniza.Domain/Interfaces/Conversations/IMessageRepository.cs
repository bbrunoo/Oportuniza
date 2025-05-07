using Oportuniza.Domain.Conversations;

namespace Oportuniza.Domain.Interfaces.Conversations
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetByConversationIdAsync(Guid conversationId, int page, int pageSize);
        Task AddAsync(Message message);
    }
}
