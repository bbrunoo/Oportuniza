using Azure;
using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Conversations;
using Oportuniza.Domain.Interfaces.Conversations;
using Oportuniza.Infrastructure.Data;
using System;

namespace Oportuniza.Infrastructure.Repositories.Conversations
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Message message)
        {
            _context.Message.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetByConversationIdAsync(Guid conversationId, int page, int pageSize)
        {
            return await _context.Message
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        }
    }
}
