using OpenAI.Chat;

namespace Oportuniza.API.Services
{
    public class OpenAIService
    {
        private readonly ChatClient _client;

        public OpenAIService()
        {
            //string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            const string apiKey = "";
            {
                throw new InvalidOperationException("A variável de ambiente 'OPENAI_API_KEY' não está definida.");
            }
            _client = new ChatClient(model: "gpt-4o", apiKey);
        }
        public async Task<string> GetFirstEmailName(string email)
        {
            string prompt = $"Extract the first valid word from this {email}, can be a name or a real word, then return it with the first letter in uppercase.";

            var completion = await _client.CompleteChatAsync(prompt);
            Console.WriteLine(completion);

            return completion.Value.Content[0].Text;
        }
    }
}
