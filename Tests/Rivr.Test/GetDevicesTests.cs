using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Devices;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class GetDevicesTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task GetDevicesAsync_WhenSuccessful_ReturnsDevices()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var devicesResponse = new GetDevicesResponse
        {
            Devices =
            [
                new Device
                {
                    DeviceUniqueId = "device-001",
                    Name = "Reception Terminal"
                },
                new Device
                {
                    DeviceUniqueId = "device-002",
                    Name = "Self-Checkout Terminal"
                }
            ]
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(devicesResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetDevicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(2);
        result[0].DeviceUniqueId.ShouldBe("device-001");
        result[0].Name.ShouldBe("Reception Terminal");
        result[1].DeviceUniqueId.ShouldBe("device-002");
        result[1].Name.ShouldBe("Self-Checkout Terminal");
    }

    [Test]
    public async Task GetDevicesAsync_WhenNoDevices_ReturnsEmptyArray()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var devicesResponse = new GetDevicesResponse
        {
            Devices = []
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(devicesResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetDevicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(0);
    }

    [Test]
    public async Task GetDevicesAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var devicesResponse = new GetDevicesResponse { Devices = [] };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(devicesResponse);
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
                .GetDevicesAsync(cts.Token));
    }
}
