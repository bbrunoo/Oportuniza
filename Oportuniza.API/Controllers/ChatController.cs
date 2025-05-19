using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oportuniza.API.Hubs;
using Oportuniza.Domain.DTOs.Chat;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using Oportuniza.Infrastructure.Repositories;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Oportuniza.API.Controllers
{
    [Route("api/chat")]
    [Authorize]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly ApplicationDbContext _context;
        public ChatController(IChatRepository chatRepository, ApplicationDbContext context)
        {
            _chatRepository = chatRepository;
            _context = context;
        }

        [HttpGet("private/{targetUserId}")]
        public async Task<IActionResult> GetPrivateChatId(Guid targetUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var userId1 = Guid.Parse(currentUserId);
            var userId2 = targetUserId;

            var currentUserName = await _chatRepository.GetUserNameById(userId1);
            var targetUserName = await _chatRepository.GetUserNameById(userId2);

            var chatId = await _chatRepository.EnsureChatAndParticipantsAsync(
                userId1, currentUserName, userId2, targetUserName);

            return Ok(new { chatId = chatId.ToString() });
        }

        //[HttpPost("create")]
        //public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        //{
        //    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(currentUserId))
        //        return Unauthorized();

        //    var userId1 = Guid.Parse(currentUserId);
        //    var userId2 = request.TargetUserId;

        //    var chatId = GenerateDeterministicChatId(userId1, userId2);

        //    var newChat = new PrivateChat
        //    {
        //        Id = chatId,
        //        User1Id = userId1,
        //        User2Id = userId2
        //    };

        //    await _chatRepository.CreateChatAsync(newChat);

        //    return Ok(chatId);
        //}

        //private static Guid GenerateDeterministicChatId(Guid user1, Guid user2)
        //{
        //    var ordered = new[] { user1, user2 }.OrderBy(g => g.ToString()).ToArray();
        //    using var md5 = System.Security.Cryptography.MD5.Create();
        //    var combinedBytes = System.Text.Encoding.UTF8.GetBytes($"{ordered[0]}_{ordered[1]}");
        //    var hashBytes = md5.ComputeHash(combinedBytes);
        //    return new Guid(hashBytes);
        //}

        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var success = await _chatRepository.DeleteMessageAsync(messageId, userId);

            if (!success) return NotFound("Mensagem não encontrada ou você não tem permissão para deletar.");

            return NoContent();
        }

        [HttpGet("history/{chatId}")]
        public async Task<IActionResult> GetChatHistory(string chatId)
        {
            var messages = await _chatRepository.GetMessagesForChatAsync(chatId);
            return Ok(messages);
        }

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetHistory(Guid userId)
        {
            var result = await _chatRepository.GetUserChatsAsync(userId);
            return Ok(result);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetUserConversations()
        {
            var userIDClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIDClaim)) return Unauthorized();

            var userId = Guid.Parse(userIDClaim);
            var result = await _chatRepository.GetUserChatsAsync(userId);
            return Ok(result);
        }
    }
}
