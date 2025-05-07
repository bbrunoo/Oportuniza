using Oportuniza.Infrastructure.Data;

namespace Oportuniza.API.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _context;
        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<Chat> GetOrCreateAsync(Guid user1Id, Guid user2Id)
        //{
        //    var chat = await _context.Chat
        //        .Include(c => c.Messages)
        //        .FirstOrDefaultAsync(c =>
        //            (c.User1Id == user1Id && c.User2Id == user2Id) ||
        //            (c.User1Id == user2Id && c.User2Id == user1Id));

        //    if (chat != null) return chat;

        //    chat = new Chat { User1Id = user1Id, User2Id = user2Id };
        //    _context.Chat.Add(chat);
        //    await _context.SaveChangesAsync();
        //    return chat;
        //}
        //public async Task<List<Message>> GetMessagesAsync(Guid chatId)
        //{
        //    return await _context.Message
        //        .Where(m => m.ChatId == chatId)
        //        .OrderBy(m => m.SentAt)
        //        .ToListAsync();
        //}
    }
}
