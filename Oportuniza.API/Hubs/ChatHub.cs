using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Infrastructure.Data;
using System.Security.Claims;

namespace Oportuniza.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserRepository _userRepository;
        public ChatHub(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        private static List<ConnectedUser> ConnectedUsers = new List<ConnectedUser>();
        private static List<ConnectedUser> ChatQueue = new List<ConnectedUser>();

        private static List<ConnectedUser> Users = new List<ConnectedUser>();
        private static List<MessageHistory> CurrentChatMessages = new List<MessageHistory>();
        private static List<ConnectedUser> CurrentChatMembers = new List<ConnectedUser>();

        private static List<MessageHistory> MessagesToSecondPerson = new List<MessageHistory>();

        private static Dictionary<string, List<ConnectedUser>> PrivateChats = new();
        private string GetPrivateChatId(Guid userId1, Guid userId2) =>
            $"chat:{(userId1.CompareTo(userId2) < 0 ? userId1 : userId2)}:{(userId1.CompareTo(userId2) < 0 ? userId2 : userId1)}";

        public override async Task OnConnectedAsync()
        {
            var identity = Context.User.Identity as ClaimsIdentity;
            var userIdClaim = identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userId = Guid.Parse(userIdClaim);
            var userInfo = await _userRepository.GetUserInfoAsync(userId);

            var connectedUser = new ConnectedUser
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                DisplayName = userInfo.Name,
            };

            if (!ConnectedUsers.Any(connectedUser => connectedUser.ConnectionId == Context.ConnectionId))
            {
                ConnectedUsers.Add(connectedUser);
            }

            await Clients.All.SendAsync("UpdateConnectedUsers", ConnectedUsers);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                ConnectedUsers.Remove(user);
                await Clients.All.SendAsync("UpdateConnectedUsers", ConnectedUsers);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            var sender = CurrentChatMembers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (sender != null)
            {
                MessagesToSecondPerson.Add(new MessageHistory()
                {
                    Message = message,
                    SentDateTime = DateTime.UtcNow,
                    SenderConnectionId = sender.ConnectionId,
                    UserName = sender.DisplayName
                });
                foreach (var user in CurrentChatMembers)
                {
                    await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessage", sender.DisplayName, message);
                }
            }
        }

        public async Task ConnectWithSecondPerson(string connectionId)
        {
            var user = Users.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId);
            var secondUser = ChatQueue.FirstOrDefault(u => u.ConnectionId == connectionId);

            if (secondUser != null && user != null)
            {
                await Clients.Client(secondUser.ConnectionId).SendAsync("StartChat", secondUser);
                await Clients.Client(user.ConnectionId).SendAsync("StartChat", user);

                if (!CurrentChatMembers.Any(a => a.ConnectionId == secondUser.ConnectionId))
                {
                    CurrentChatMembers.Add(secondUser);
                }

                if (!CurrentChatMembers.Any(a => a.ConnectionId == user.ConnectionId))
                {
                    CurrentChatMembers.Add(user);
                }

                var initialMessage = new MessageHistory() { Message = $"Iniciando conversa com {secondUser.DisplayName}" };
                CurrentChatMessages.Add(initialMessage);

                await Clients.Client(secondUser.ConnectionId).SendAsync("ReceiveMessage", "System", initialMessage.Message);
                await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessage", "System", initialMessage.Message);

                ChatQueue.Remove(secondUser);
                await Clients.All.SendAsync("UpdateChatQueue", ChatQueue);
            }
        }
        public async Task JoinChatQueue()
        {
            var user = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                if (!ChatQueue.Any(a => a.ConnectionId == Context.ConnectionId))
                {
                    ChatQueue.Add(user);
                }
                await Clients.All.SendAsync("UpdateChatQueue", ChatQueue);
            }
        }

        public async Task StartPrivateChat(Guid targetUserId)
        {
            var userA = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            var userB = ConnectedUsers.FirstOrDefault(u => u.UserId == targetUserId);

            if (userA == null)
                throw new HubException("Usuário atual não encontrado na lista de conexões.");

            if (userB == null)
                throw new HubException("Usuário de destino não encontrado na lista de conexões.");

            var chatId = GetPrivateChatId(userA.UserId, userB.UserId);

            if (!PrivateChats.ContainsKey(chatId))
                PrivateChats[chatId] = new List<ConnectedUser>();

            if (!PrivateChats[chatId].Any(u => u.ConnectionId == userA.ConnectionId))
                PrivateChats[chatId].Add(userA);

            if (!PrivateChats[chatId].Any(u => u.ConnectionId == userB.ConnectionId))
                PrivateChats[chatId].Add(userB);

            foreach (var user in PrivateChats[chatId])
            {
                await Clients.Client(user.ConnectionId).SendAsync("StartChat", userB);
            }
        }
        public async Task SendPrivateMessage(Guid toUserId, string message)
        {
            var fromUser = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            var toUser = ConnectedUsers.FirstOrDefault(u => u.UserId == toUserId);
            if (fromUser == null || toUser == null) return;

            var chatId = GetPrivateChatId(fromUser.UserId, toUser.UserId);
            if (!PrivateChats.ContainsKey(chatId)) return;

            foreach (var user in PrivateChats[chatId])
            {
                await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessage", fromUser.DisplayName, message);
            }
        }

        public async Task EndChat()
        {
            foreach (var secondUser in CurrentChatMembers)
            {
                await Clients.Client(secondUser.ConnectionId).SendAsync("EndChat", secondUser);
            }
            CurrentChatMembers.Clear();
            CurrentChatMessages.Clear();
        }
        private class ConnectedUser
        {
            public string ConnectionId { get; set; }
            public Guid UserId { get; set; }
            public string DisplayName { get; set; }
            public bool IsAdmin { get; set; }
        }

        private class MessageHistory
        {
            public string? SenderConnectionId { get; set; }
            public string? Message { get; set; }
            public DateTime SentDateTime { get; set; } = DateTime.UtcNow;
            public string? UserName { get; set; } = string.Empty;
        }
    }
}
