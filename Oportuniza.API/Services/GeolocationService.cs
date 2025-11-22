using System.Text.Json;

namespace Oportuniza.API.Services
{
    public class GeolocationService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "41f15f377689480cb178e4951adf7285";
        public GeolocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string location)
        {
            var encodedLocation = Uri.EscapeDataString(location);
            var url = $"https://api.opencagedata.com/geocode/v1/json?q={encodedLocation}&key={ApiKey}&limit=1";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao buscar coordenadas. Código: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement.GetProperty("results");

            if (results.GetArrayLength() == 0)
                throw new Exception("Localização não encontrada.");

            var geometry = results[0].GetProperty("geometry");
            double lat = geometry.GetProperty("lat").GetDouble();
            double lng = geometry.GetProperty("lng").GetDouble();

            return (lat, lng);
        }

        private class NominatimResult
        {
            public string lat { get; set; }
            public string lon { get; set; }
        }
    }
}
