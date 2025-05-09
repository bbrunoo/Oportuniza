using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Oportuniza.Domain.Interfaces;
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

        public async Task JoinChat(Guid chatId)
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

            if (!ConnectedUsers.Any(u => u.ConnectionId == connectedUser.ConnectionId))
                ConnectedUsers.Add(connectedUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

            await Clients.Group(chatId.ToString()).SendAsync("UserJoined", connectedUser.DisplayName);
        }
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
        public async Task SendMessageToChat(Guid chatId, string message)
        {
            var sender = ConnectedUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (sender == null) return;

            var messageHistory = new MessageHistory
            {
                Message = message,
                SentDateTime = DateTime.UtcNow,
                SenderConnectionId = sender.ConnectionId,
                UserName = sender.DisplayName
            };

            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", sender.DisplayName, message);
        }
        public string GetPrivateChatId(Guid userId1, Guid useriId2)
        {
            var sorted = new[] { userId1, useriId2 }.OrderBy(id => id).ToArray();
            var combined = $"{sorted[0]}-{sorted[1]}";

            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));

            var bigInt = new System.Numerics.BigInteger(hashBytes.Append((byte)0).ToArray());
            var numericString = System.Numerics.BigInteger.Abs(bigInt).ToString();

            return numericString.Length >= 32
                ? numericString.Substring(0, 32)
                : numericString.PadLeft(32, '0');
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


