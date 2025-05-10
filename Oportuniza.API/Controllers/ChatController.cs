using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Hubs;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Infrastructure.Repositories;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/chat")]
    [Authorize]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private static IChatRepository _chatRepository;
        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpGet("private/{targetUserId}")]
        public IActionResult GetPrivateChatId(Guid targetUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var userId1 = Guid.Parse(currentUserId);
            var userId2 = targetUserId;

            var chatId = GenerateDeterministicChatId(userId1, userId2);
            return Ok(chatId);
        }
        private static Guid GenerateDeterministicChatId(Guid user1, Guid user2)
        {
            var ordered = new[] { user1, user2 }.OrderBy(g => g.ToString()).ToArray();
            using var md5 = System.Security.Cryptography.MD5.Create();
            var combinedBytes = System.Text.Encoding.UTF8.GetBytes($"{ordered[0]}_{ordered[1]}");
            var hashBytes = md5.ComputeHash(combinedBytes);
            return new Guid(hashBytes);
        }

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
        public async Task<IActionResult> GetChatHistory(Guid userId)
        {
            var result = await _chatRepository.GetUserChatsAsync(userId);
            return Ok(result);
        }
    }
}
