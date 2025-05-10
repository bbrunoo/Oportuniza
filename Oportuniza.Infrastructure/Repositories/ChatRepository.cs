using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Chat;
using Oportuniza.Domain.DTOs.User;
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

        public async Task<string> EnsureChatAndParticipantsAsync(Guid userAId, string userAName, Guid userBId, string userBName)
        {
            var chatId = GenerateDeterministicChatId(userAId, userBId);

            var existingParticipants = await _context.ChatParticipants
                .Where(p => p.ChatId == chatId.ToString())
                .ToListAsync();

            if (existingParticipants.Count == 0)
            {
                var participants = new List<ChatParticipant>
        {
            new ChatParticipant { ChatId = chatId.ToString(), UserId = userAId, UserName = userAName },
            new ChatParticipant { ChatId = chatId.ToString(), UserId = userBId, UserName = userBName }
        };

                await _context.ChatParticipants.AddRangeAsync(participants);
                await _context.SaveChangesAsync();
            }

            return chatId.ToString();
        }

        private static Guid GenerateDeterministicChatId(Guid user1, Guid user2)
        {
            var ordered = new[] { user1, user2 }.OrderBy(g => g.ToString()).ToArray();
            using var md5 = System.Security.Cryptography.MD5.Create();
            var combinedBytes = System.Text.Encoding.UTF8.GetBytes($"{ordered[0]}_{ordered[1]}");
            var hashBytes = md5.ComputeHash(combinedBytes);
            return new Guid(hashBytes);
        }

        public async Task<List<ChatMessage>> GetMessagesForChatAsync(string chatId)
        {
            return await _context.ChatMessage
                        .Where(m => m.ChatId == chatId)
                        .OrderBy(m => m.SentAt)
                        .ToListAsync();
        }

        public async Task<List<ChatHistoryDto>> GetUserChatsAsync(Guid userId)
        {
            var userChats = await _context.ChatParticipants
                .Where(p => p.UserId == userId)
                .Select(p => p.ChatId)
                .ToListAsync();

            var chats = await _context.ChatParticipants
                .Where(p => userChats.Contains(p.ChatId))
                .GroupBy(p => p.ChatId)
                .Select(g => new ChatHistoryDto
                {
                    ChatId = g.Key,
                    Participants = g.Where(p => p.UserId != userId).Select(p => p.UserName).ToList(),
                    LastMessage = _context.ChatMessage
                        .Where(m => m.ChatId == g.Key)
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Message)
                        .FirstOrDefault(),
                    LastMessageDate = _context.ChatMessage
                        .Where(m => m.ChatId == g.Key)
                        .Max(m => (DateTime?)m.SentAt)
                })
                .ToListAsync();

            return chats;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _context.ChatMessage.AddAsync(message);
            await _context.SaveChangesAsync();
        }
    }
}
