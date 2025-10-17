using System.Net.Http.Headers;

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
        var tokenEndpoint = "https://keycloak.oportuniza.site/realms/oportuniza/protocol/openid-connect/token";

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler);

        var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("grant_type", "password"),
        new KeyValuePair<string, string>("client_id", "oportuniza-client"),
        new KeyValuePair<string, string>("client_secret", "RMNT9CAC1mHULvD5vhhyg80NlFX1Kftt"),
        new KeyValuePair<string, string>("username", email),
        new KeyValuePair<string, string>("password", password),
    });

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = form
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[KeycloakAuthService] Erro: {response.StatusCode} - {content}");
            return null;
        }

        return content;
    }


    //public async Task<string?> LoginWithCredentialsAsync(string email, string password)
    //{
    //    var tokenEndpoint = "https://keycloak.oportuniza.site/realms/oportuniza/protocol/openid-connect/token";

    //    var form = new FormUrlEncodedContent(new[]
    //    {
    //        new KeyValuePair<string, string>("grant_type", "password"),
    //        new KeyValuePair<string, string>("client_id", "oportuniza-client"),
    //        new KeyValuePair<string, string>("client_secret", "RMNT9CAC1mHULvD5vhhyg80NlFX1Kftt"),
    //        new KeyValuePair<string, string>("username", email),
    //        new KeyValuePair<string, string>("password", password),
    //    });

    //    var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
    //    {
    //        Content = form
    //    };

    //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    //    var response = await _httpClient.SendAsync(request);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        return null;
    //    }

    //    var content = await response.Content.ReadAsStringAsync();
    //    return content;
    //}
}
