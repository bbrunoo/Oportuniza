using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oportuniza.API.Services
{
    public class CNPJService
    {
        private readonly HttpClient _httpClient;

        public CNPJService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public bool IsValid(string cnpj)
        {
            if (cnpj.Length != 14) return false;

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }

        //====================================
        public class CnpjStatusDto
        {
            [JsonPropertyName("cnpj")]
            public string Cnpj { get; set; } = string.Empty;

            [JsonPropertyName("descricao_situacao_cadastral")]
            public string SituacaoCadastral { get; set; } = string.Empty;

            public bool Ativo => SituacaoCadastral?.Trim().ToUpper() == "ATIVA";
        }

        public async Task<bool> VerificarAtividadeCnpjAsync(string cnpj)
        {
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (!IsValid(cnpj))
                throw new ArgumentException("CNPJ inválido.");

            var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpj}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao consultar CNPJ: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("descricao_situacao_cadastral", out var situacao))
            {
                var status = situacao.GetString()?.ToLowerInvariant();
                return status == "ativa";
            }

            return false;
        }
    }
}
