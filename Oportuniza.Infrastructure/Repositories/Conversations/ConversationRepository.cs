using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Conversations;
using Oportuniza.Domain.Interfaces.Conversations;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories.Conversations
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ApplicationDbContext _context;
        public ConversationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Conversation conversation)
        {
            _context.Conversation.Add(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task<Conversation> FindByParticipantsAsync(List<long> participantIds)
        {
            var ordered = participantIds.OrderBy(id => id).ToList();

            var candidates = await _context.Conversation
                .Where(c => !c.RoomId.HasValue)
                .Include(c => c.Participants)
                .ToListAsync();

            return candidates.FirstOrDefault(c =>
                 c.Participants.Count == ordered.Count &&
                 c.Participants.Select(p => p.UserId).OrderBy(id => id).SequenceEqual(ordered));
        }
                          
        public async Task<Conversation> GetByIdAsync(Guid id)
        {
            return await _context.Conversation.FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
