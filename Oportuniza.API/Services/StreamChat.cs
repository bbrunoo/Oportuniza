using StreamChat.Clients;

namespace Oportuniza.API.Services
{
    public class StreamChat
    {

        public StreamChat(IConfiguration config)
        {
            var apiKey = config["StreamChat:ApiKey"];
            var apiSecret = config["StreamChat:ApiSecret"];
            var clientFactory = new StreamClientFactory(apiKey, apiSecret);
        }

        public async Task CreateUserIfNotExistsAsync(string userId, string name)
        {

        }
    }
}
