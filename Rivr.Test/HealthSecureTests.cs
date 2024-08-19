using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Models;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class HealthSecureTests
{
    private Config _config;

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task ShouldReturnHealth()
    {
        // Arrange
        var authReponse = new TokenResponse();
        var healthResponse = new Health { Message = "OK" };

        var authHandler = new MockHttpMessageHandler(authReponse);
        var apiHandler = new MockHttpMessageHandler(healthResponse);

        var authHttpHandler = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);

        var memoryCache = Substitute.For<IMemoryCache>();

        var client = new Client(authHttpHandler, apiHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsPlatform()
            .GetHealthSecureAsync();

        // Assert
        var authRequestContent = authHandler.GetRequestContent<ClientCredentialsTokenRequest>();
        authRequestContent.ShouldNotBeNull();
        authRequestContent.GrantType.ShouldBe("client_credentials");

        result.ShouldNotBeNull();
        result.Message.ShouldBe("OK");
    }
}