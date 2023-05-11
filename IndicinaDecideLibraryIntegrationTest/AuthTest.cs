#nullable disable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using IndicinaDecideLibrary.Auth;
using Microsoft.Extensions.Configuration;

namespace IndicinaDecideLibraryIntegrationTests;

[TestClass]
public class AuthManagerIntegrationTest
{
    private AuthManager _authManager;
    private string _client_id;
    private string _client_secret;

    [TestInitialize]
    public void TestInitialize()
    {
        var httpClient = new HttpClient();

        var configuration = new ConfigurationBuilder()
        .AddUserSecrets<AuthManagerIntegrationTest>()
        .Build();

        _client_id = configuration["IndicinaClientId"];
        _client_secret = configuration["IndicinaClientSecret"];

        if (_client_id == null || _client_secret == null)
        {
            throw new Exception("Environment variables not set");
        }

        _authManager = new AuthManager(_client_id, _client_secret, httpClient);
    }

    [TestMethod]
    public async Task GetAuthCodeAsync_ReturnsAuthCode()
    {
        // Act
        string authCode = await _authManager.GetAuthCodeAsync();

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(authCode));
    }

    [TestMethod]
    public async Task RefreshAuthCodeAsync_UpdatesAuthCode()
    {
        // Arrange
        string initialAuthCode = await _authManager.GetAuthCodeAsync();

        // Act
        await _authManager.RefreshAuthCodeAsync();
        string refreshedAuthCode = await _authManager.GetAuthCodeAsync();

        // Assert
        Assert.AreNotEqual(initialAuthCode, refreshedAuthCode);
    }
}

