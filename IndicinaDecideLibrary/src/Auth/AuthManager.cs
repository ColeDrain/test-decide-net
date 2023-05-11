namespace IndicinaDecideLibrary.Auth;


using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class AuthManager
{
    //private const string LoginUrl = "https://api.indicina.co/api/v3/client/api/login";
    private const string TokenGeneratorURL = "https://staging-decide-api.indicina.net/account/token/generate";
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _authCode;
    private readonly HttpClient _httpClient;

    public record Data([property: JsonPropertyName("token")] string Token);

    public record TokenResponse([property: JsonPropertyName("statusCode")] int StatusCode, [property: JsonPropertyName("data")] Data? Data, [property: JsonPropertyName("message")] string? Message);

    public AuthManager(string clientId, string clientSecret, HttpClient httpClient)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _httpClient = httpClient;
    }

    public async Task<string> GetAuthCodeAsync()
    {
        if (string.IsNullOrEmpty(_authCode))
        {
            try
            {
                _authCode = await FetchAuthCodeAsync(TokenGeneratorURL);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching auth code in GetAuthCodeAsync.", ex);
            }
        }
        return _authCode ?? string.Empty;
    }

    public async Task RefreshAuthCodeAsync()
    {
        _authCode = await FetchAuthCodeAsync(TokenGeneratorURL);
    }

    private async Task<string?> FetchAuthCodeAsync(string url)
    {
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new
        {
            client_id = _clientId,
            client_secret = _clientSecret
        };
        string requestBodyJson = JsonSerializer.Serialize(requestBody);

        StringContent requestContent = new StringContent(requestBodyJson, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, requestContent);

        _ = response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        TokenResponse? responseObject = JsonSerializer.Deserialize<TokenResponse>(responseBody);

        return responseObject?.StatusCode is not 200
            ? throw new Exception($"Unable to fetch auth code. {nameof(responseObject.StatusCode)}: {responseObject?.StatusCode}")
            : responseObject?.Data?.Token;
    }
}
