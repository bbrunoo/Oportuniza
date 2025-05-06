using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Claims;

namespace Oportuniza.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        public ChatHub(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        private static List<ConnectedUser> ConnectedUsers = new List<ConnectedUser>();
        public async Task JoinChat(string chatId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                Console.WriteLine($"[ChatHub] Usuário {Context.ConnectionId} entrou no chat {chatId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JoinChat failed: {ex.Message}");
                throw; // Re-throw so client sees error
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return;

            var userId = Guid.Parse(userIdClaim);
            var userInfo = await _userRepository.GetUserInfoAsync(userId);

            if (userInfo == null) return;

            if (!ConnectedUsers.Any(u => u.UserId == userId))
            {
                ConnectedUsers.Add(new ConnectedUser
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    DisplayName = userInfo.Name
                });

                Console.WriteLine($"[ChatHub] {userInfo.Name} conectado com ID {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                ConnectedUsers.Remove(user);
                Console.WriteLine($"[ChatHub] {user.DisplayName} desconectado.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToChat(string chatId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            var sender = ConnectedUsers.FirstOrDefault(u => u.UserId == Guid.Parse(userId));
            if (sender == null) return;

            var chatMessage = new ChatMessage
            {
                ChatId = chatId,
                SenderId = sender.UserId,
                SenderName = sender.DisplayName,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            await _chatRepository.SaveMessageAsync(chatMessage);
            await Clients.Group(chatId).SendAsync("ReceiveMessage", new
            {
                Id = chatMessage.Id,
                ChatId = chatMessage.ChatId,
                SenderId = chatMessage.SenderId,
                SenderName = chatMessage.SenderName,
                Message = chatMessage.Message,
                SentAt = chatMessage.SentAt
            });
        }

        private class ConnectedUser
        {
            public string ConnectionId { get; set; }
            public Guid UserId { get; set; }
            public string DisplayName { get; set; }
        }
    }
}


