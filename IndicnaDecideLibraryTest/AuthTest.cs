#nullable disable

using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using IndicinaDecideLibrary.Auth;
using System.Net;
using System.Threading;

namespace IndicinaDecideLibraryUnitTests
{
    [TestClass]
    public class AuthManagerUnitTest
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private AuthManager _authManager;

        [TestInitialize]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _authManager = new AuthManager("testClientId", "testClientSecret", httpClient);
        }

        [TestMethod]
        public async Task GetAuthCodeAsync_ReturnsExpectedAuthCode()
        {
            // Arrange
            var expectedAuthCode = "testAuthCode";
            var responseContent = new StringContent("{\"statusCode\": 200, \"data\": {\"token\": \"" + expectedAuthCode + "\"}, \"message\": null}", Encoding.UTF8, "application/json");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = responseContent };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _authManager.GetAuthCodeAsync();

            // Assert
            Assert.AreEqual(expectedAuthCode, result);
        }

        [TestMethod]
        public async Task RefreshAuthCodeAsync_UpdatesAuthCode()
        {
            // Arrange
            var expectedAuthCode = "newAuthCode";
            var responseContent = new StringContent("{\"statusCode\": 200, \"data\": {\"token\": \"" + expectedAuthCode + "\"}, \"message\": null}", Encoding.UTF8, "application/json");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = responseContent };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            await _authManager.RefreshAuthCodeAsync();
            var result = await _authManager.GetAuthCodeAsync();

            // Assert
            Assert.AreEqual(expectedAuthCode, result);
        }
    }
}
