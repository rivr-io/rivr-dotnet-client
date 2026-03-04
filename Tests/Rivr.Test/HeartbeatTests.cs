using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Heartbeats;
using Rivr.Core.Models.SatelliteServices;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class HeartbeatTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task SendHeartbeatAsync_WhenSuccessful_ReturnsResponse()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var heartbeatResponse = new HeartbeatResponse
        {
            Configuration = new SatelliteServiceConfiguration()
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(heartbeatResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var heartbeat = new SendHeartbeatRequest
        {
            UniqueServiceId = "service-001",
            ServiceName = "Test Service",
            Version = "1.0.0"
        };

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .SendHeartbeatAsync(heartbeat);

        // Assert
        result.ShouldNotBeNull();
        result.Configuration.ShouldNotBeNull();
        apiHandler.PerformedRequestsCount.ShouldBe(1);
    }

    [Test]
    public async Task SendHeartbeatAsync_WhenNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler();
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .SendHeartbeatAsync(null!));
    }

    [Test]
    public async Task SendHeartbeatAsync_WhenMissingUniqueServiceId_ThrowsArgumentException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler();
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var heartbeat = new SendHeartbeatRequest
        {
            UniqueServiceId = "",
            ServiceName = "Test Service"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .SendHeartbeatAsync(heartbeat));

        exception.Message.ShouldContain("UniqueServiceId is required");
    }

    [Test]
    public async Task SendHeartbeatAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var heartbeatResponse = new HeartbeatResponse();

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(heartbeatResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var heartbeat = new SendHeartbeatRequest
        {
            UniqueServiceId = "service-001",
            ServiceName = "Test Service"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .SendHeartbeatAsync(heartbeat, cts.Token));
    }
}
