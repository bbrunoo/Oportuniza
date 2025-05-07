using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task SendMessage(Guid chatId, string message)
        //{
        //    try
        //    {
        //        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (!Guid.TryParse(userIdClaim, out var userId))
        //        {
        //            Console.WriteLine("[SendMessage] Falha ao extrair o ID do usuário.");
        //            throw new HubException("Usuário não autenticado.");
        //        }

        //        var chat = await _context.Chat.FindAsync(chatId);
        //        if (chat == null)
        //        {
        //            Console.WriteLine($"[SendMessage] Chat com ID {chatId} não encontrado.");
        //            throw new HubException("Chat não encontrado.");
        //        }

        //        var newMessage = new Message
        //        {
        //            ChatId = chatId,
        //            Text = message,
        //            SenderId = userId,
        //            SentAt = DateTime.UtcNow
        //        };

        //        _context.Message.Add(newMessage);
        //        await _context.SaveChangesAsync();

        //        var messageDto = new
        //        {
        //            newMessage.Id,
        //            newMessage.ChatId,
        //            newMessage.Text,
        //            newMessage.SenderId,
        //            newMessage.SentAt
        //        };

        //        Console.WriteLine($"[SendMessage] Mensagem enviada por {userId} no chat {chatId}");

        //        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[SendMessage] Erro: {ex.Message}");
        //        throw new HubException($"Erro ao enviar mensagem: {ex.Message}");
        //    }
        //}
        public async Task SendMessageToGroup(string user, string message)
        => await Clients.Group("SignalR Users").SendAsync("ReceiveMessage", user, message);
        public async Task JoinChat(Guid chatId)
        {
            try
            {
                var userId = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("JoinChat: Usuário não autenticado ou claim não encontrada.");
                    throw new HubException("Usuário não autenticado.");
                }

                Console.WriteLine($"Usuário {userId} entrando no chat {chatId}");

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em JoinChat: {ex.Message}");
                throw;
            }
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

    }
}
