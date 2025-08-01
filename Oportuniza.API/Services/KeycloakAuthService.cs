using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

public class KeycloakAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public KeycloakAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string?> LoginWithCredentialsAsync(string email, string password)
    {
        var tokenEndpoint = "http://localhost:9090/realms/oportuniza/protocol/openid-connect/token";

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "oportuniza-client"),
            new KeyValuePair<string, string>("client_secret", "Sr1LFcfOHwtFckn8HAHHAf7IxklDiBI3"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
        });

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = form
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
}
