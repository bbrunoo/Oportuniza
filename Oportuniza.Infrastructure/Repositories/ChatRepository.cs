using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.ChatMessage
                 .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId);

            if (message == null)
                return false;

            _context.ChatMessage.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<ChatMessage>> GetMessagesForChatAsync(string chatId)
        {
            return await _context.ChatMessage
                        .Where(m => m.ChatId == chatId)
                        .OrderBy(m => m.SentAt)
                        .ToListAsync();
        }
        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _context.ChatMessage.AddAsync(message);
            await _context.SaveChangesAsync();
        }
    }
}
