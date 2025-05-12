using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Chat;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using System;

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

        public async Task<List<ChatSummaryDto>> GetUserChatsAsync(Guid userId)
        {
            var userChats = await _context.ChatParticipants
                .Where(p => p.UserId == userId)
                .Select(p => p.ChatId)
                .ToListAsync();

            var chatSummaries = await _context.ChatParticipants
                .Where(p => userChats.Contains(p.ChatId))
                .Where(p => p.UserId != userId)
                .Select(p => new ChatSummaryDto
                {
                    ChatId = p.ChatId,
                    TargetUserId = p.UserId,
                    TargetUserName = p.UserName,
                    LastMessage = _context.ChatMessage
                    .Where(m=>m.ChatId == p.ChatId)
                    .OrderByDescending(m=>m.SentAt)
                    .Select(m=>m.Message)
                    .FirstOrDefault(),
                    LastMessageDate = _context.ChatMessage
                    .Where(m=>m.ChatId == p.ChatId)
                    .Max(m => (DateTime?)m.SentAt)
                })
                .ToListAsync();

            return chatSummaries;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _context.ChatMessage.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ChatExistsAsync(Guid chatId)
        {
            return await _context.PrivateChat.AnyAsync(c => c.Id == chatId);
        }
        public async Task CreateChatAsync(PrivateChat chat)
        {
            _context.PrivateChat.Add(chat);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetUserNameById(Guid userId)
        {
            var user = await _context.User
                        .Where(u => u.Id == userId)
                        .Select(u => u.Name)
                        .FirstOrDefaultAsync();

            return user ?? "Desconhecido";
        }
    }
}
