using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Core.Models;
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
        var healthResponse = new Health { Message = "OK" };

        var authHttpHandler = new HttpClient(new MockHttpMessageHandler());
        var apiHttpClient = new HttpClient(new MockHttpMessageHandler(healthResponse));

        var memoryCache = Substitute.For<IMemoryCache>();

        var client = new Client(authHttpHandler, apiHttpClient, _config, memoryCache);

        // Act
        var result = await client.GetHealthAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("OK");
    }
}