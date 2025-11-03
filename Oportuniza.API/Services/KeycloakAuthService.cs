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
        var tokenEndpoint = "https://auth.oportuniza.site/realms/oportuniza/protocol/openid-connect/token";
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler);

        var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("grant_type", "password"),
        new KeyValuePair<string, string>("client_id", "oportuniza-client"),
        new KeyValuePair<string, string>("client_secret", "yfq4temRzipAqpkQvobcCY78ES7wH3G4"),
        new KeyValuePair<string, string>("username", email),
        new KeyValuePair<string, string>("password", password),
    });

        var response = await client.PostAsync(tokenEndpoint, form);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[KeycloakAuthService] Erro: {response.StatusCode} - {content}");
            return null;
        }

        var tokenJson = System.Text.Json.JsonDocument.Parse(content);
        var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();

        if (string.IsNullOrEmpty(accessToken))
            return null;

        var isVerified = await IsEmailVerifiedAsync(email, accessToken);
        if (!isVerified)
        {
            Console.WriteLine($"[KeycloakAuthService] E-mail de {email} não verificado.");
            return "NOT_VERIFIED";
        }

        return content;
    }

    private async Task<bool> IsEmailVerifiedAsync(string email, string accessToken)
    {
        var adminToken = await GetAdminToken();
        using var client = new HttpClient();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://auth.oportuniza.site/admin/realms/oportuniza/users?email={email}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return false;

        dynamic users = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;
        if (users.Count == 0)
            return false;

        return users[0].emailVerified == true;
    }

    private async Task<string> GetAdminToken()
    {
        using var client = new HttpClient();

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = "admin",
            ["password"] = "admin"
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await client.PostAsync(
            "https://auth.oportuniza.site/realms/master/protocol/openid-connect/token",
            content
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;
        return obj.access_token;
    }
}
