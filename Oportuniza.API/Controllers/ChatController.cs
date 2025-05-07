using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        //[HttpGet("get-or-create/{user1Id}/{user2Id}")]
        //public async Task<IActionResult> GetOrCreateChat(Guid user1Id, Guid user2Id)
        //{
        //    var chat = await _chatService.GetOrCreateAsync(user1Id, user2Id);
        //    return Ok(chat);
        //}

        //[HttpGet("messages/{chatId}")]
        //public async Task<IActionResult> GetMessages(Guid chatId)
        //{
        //    var messages = await _chatService.GetMessagesAsync(chatId);
        //    return Ok(messages);
        //}
    }
}
