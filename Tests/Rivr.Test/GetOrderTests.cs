using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class GetOrderTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task GetOrderAsync_WhenSuccessful_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var orderResponse = new Order
        {
            Id = orderId,
            MerchantId = _merchantId,
            Reference = "order-123",
            Amount = 250,
            Status = OrderStatus.Completed,
            PaymentMethod = PaymentMethod.Card
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(orderResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetOrderAsync(orderId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(orderId);
        result.Reference.ShouldBe("order-123");
        result.Amount.ShouldBe(250);
        result.Status.ShouldBe(OrderStatus.Completed);
        result.PaymentMethod.ShouldBe(PaymentMethod.Card);
    }

    [Test]
    public async Task GetOrderStatusAsync_WhenSuccessful_ReturnsStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var statusResponse = new OrderStatusOnly
        {
            Status = OrderStatus.Completed
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(statusResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetOrderStatusAsync(orderId);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(OrderStatus.Completed);
    }

    [Test]
    public async Task GetOrderAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var orderResponse = new Order { Id = orderId };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(orderResponse);
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
                .GetOrderAsync(orderId, cts.Token));
    }
}
