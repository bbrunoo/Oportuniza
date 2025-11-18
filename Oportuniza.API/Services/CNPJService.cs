using Oportuniza.Domain.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oportuniza.API.Services
{
    public class CNPJService
    {
        private readonly HttpClient _http;
        private readonly ICnpjCacheRepository _repo;

        public CNPJService(HttpClient http, ICnpjCacheRepository repo)
        {
            _http = http;
            _repo = repo;
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

        public class CnpjStatusDto
        {
            [JsonPropertyName("cnpj")]
            public string Cnpj { get; set; } = string.Empty;

            [JsonPropertyName("descricao_situacao_cadastral")]
            public string SituacaoCadastral { get; set; } = string.Empty;

            public bool Ativo => SituacaoCadastral?.Trim().ToUpper() == "ATIVA";
        }

        public async Task<bool?> VerificarAtividadeCnpjAsync(string cnpj)
        {
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());
            if (cnpj.Length != 14) return null;

            var cache = await _repo.GetAsync(cnpj);
            if (cache != null && cache.AtualizadoEm > DateTime.UtcNow.AddDays(-30))
                return cache.Situacao.ToUpper() == "ATIVA";

            try
            {
                var url = $"https://publica.cnpj.ws/cnpj/{cnpj}";

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Erro API pública CNPJ.ws: {response.StatusCode}");

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var status = doc.RootElement
                    .GetProperty("estabelecimento")
                    .GetProperty("situacao_cadastral")
                    .GetString()?
                    .Trim()
                    .ToUpper() ?? "";

                await _repo.UpsertAsync(cnpj, status);

                return status == "ATIVA";
            }
            catch
            {
                if (cache != null)
                    return cache.Situacao.ToUpper() == "ATIVA";

                return null;
            }
        }
    }
}
