using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Core.Models;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class HealthTests
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
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpHandler = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = Substitute.For<IMemoryCache>();

        var client = new Client(authHttpHandler, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client.GetHealthAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("OK");
    }
}