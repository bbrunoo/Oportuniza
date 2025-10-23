using System.Text.Json;

namespace Oportuniza.API.Services
{
    public class GeolocationService
    {
        private readonly HttpClient _httpClient;

        public GeolocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string location)
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(location)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Falha ao obter coordenadas de localização.");

            var json = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);

            if (results == null || results.Count == 0)
                throw new Exception("Localização não encontrada.");

            double lat = double.Parse(results[0].lat, System.Globalization.CultureInfo.InvariantCulture);
            double lon = double.Parse(results[0].lon, System.Globalization.CultureInfo.InvariantCulture);

            return (lat, lon);
        }

        private class NominatimResult
        {
            public string lat { get; set; }
            public string lon { get; set; }
        }
    }
}
