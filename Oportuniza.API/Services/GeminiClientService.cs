namespace Oportuniza.API.Services
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Text;

    namespace Oportuniza.API.Services
    {
        public class GeminiClientService
        {
            private readonly HttpClient _httpClient;
            private readonly string _apiKey;

            public GeminiClientService(HttpClient httpClient, IConfiguration configuration)
            {
                _httpClient = httpClient;
                _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey não configurada.");
            }

            public async Task<string> CreateSummaryAsync(string description, string shift, string local, string contract, int maxWords, string salary)
            {
                var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

                var prompt = $"Crie um resumo da vaga em no máximo {maxWords} palavras baseado nos seguintes dados: descrição: {description}, turno: {shift}, local: {local}, tipo de contrato: {contract}, salario: {salary}. O resumo deve ser direto e simples, sem caracteres especiais como [] ou {{}}.";

                var requestBody = new
                {
                    contents = new[]
                    {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
                };

                var jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro Gemini API: {response.StatusCode} - {responseString}");
                }

                dynamic responseObject = JsonConvert.DeserializeObject(responseString)
                    ?? throw new Exception("Resposta inválida da Gemini API");

                string summary = responseObject.candidates[0].content.parts[0].text;

                return summary.Trim();
            }
        }
    }
}