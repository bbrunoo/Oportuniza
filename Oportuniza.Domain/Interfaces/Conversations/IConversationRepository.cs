
using Oportuniza.Domain.Conversations;

namespace Oportuniza.Domain.Interfaces.Conversations
{
    public interface IConversationRepository
    {
        Task<Conversation> FindByParticipantsAsync(List<long> participantIds);
        Task AddAsync(Conversation conversation);
        Task<Conversation> GetByIdAsync(Guid id);
    }
}
