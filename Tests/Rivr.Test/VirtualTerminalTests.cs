using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.SatelliteServices;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class VirtualTerminalTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task GetVirtualTerminalsAsync_WhenSuccessful_ReturnsTerminals()
    {
        // Arrange
        var terminalId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var terminalsResponse = new GetVirtualTerminalsResponse
        {
            VirtualTerminals =
            [
                new VirtualTerminal
                {
                    Id = terminalId,
                    Name = "Terminal 1",
                    POIID = "POI-001",
                    IsActive = true,
                    MerchantId = _merchantId
                },
                new VirtualTerminal
                {
                    Id = Guid.NewGuid(),
                    Name = "Terminal 2",
                    POIID = "POI-002",
                    IsActive = false,
                    MerchantId = _merchantId
                }
            ]
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(terminalsResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetVirtualTerminalsAsync("service-001");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(2);
        result[0].Id.ShouldBe(terminalId);
        result[0].Name.ShouldBe("Terminal 1");
        result[0].POIID.ShouldBe("POI-001");
        result[0].IsActive.ShouldBeTrue();
        result[1].Name.ShouldBe("Terminal 2");
        result[1].IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task GetVirtualTerminalsAsync_WhenNoTerminals_ReturnsEmptyArray()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var terminalsResponse = new GetVirtualTerminalsResponse
        {
            VirtualTerminals = []
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(terminalsResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetVirtualTerminalsAsync("service-001");

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(0);
    }

    [Test]
    public async Task GetVirtualTerminalsAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var terminalsResponse = new GetVirtualTerminalsResponse { VirtualTerminals = [] };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(terminalsResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetVirtualTerminalsAsync("service-001", cts.Token));
    }
}
