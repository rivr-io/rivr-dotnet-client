using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class RefundTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task RefundAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.OK);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert - Should not throw
        await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .RefundAsync(orderId);

        apiHandler.PerformedRequestsCount.ShouldBe(1);
    }

    [Test]
    public async Task RefundAsync_WhenBadRequest_ThrowsApiCallException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var errorResponse = new ApiErrorResponse { Message = "Order cannot be refunded" };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(errorResponse, System.Net.HttpStatusCode.BadRequest);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiCallException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .RefundAsync(orderId));

        exception.Message.ShouldBe("Order cannot be refunded");
    }

    [Test]
    public async Task RefundAsync_WhenBadRequestWithNullMessage_ThrowsUnknownError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var errorResponse = new ApiErrorResponse { Message = null };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(errorResponse, System.Net.HttpStatusCode.BadRequest);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert
        var exception = await Should.ThrowAsync<ApiCallException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .RefundAsync(orderId));

        exception.Message.ShouldBe("Unknown error");
    }

    [Test]
    public async Task RefundAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.OK);
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
                .RefundAsync(orderId, cts.Token));
    }
}
