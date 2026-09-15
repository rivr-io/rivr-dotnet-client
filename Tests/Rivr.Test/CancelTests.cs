using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class CancelTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    private IMerchantOperations MerchantWith(MockHttpMessageHandler apiHandler)
    {
        var authHttpClient = new HttpClient(new MockHttpMessageHandler(new TokenResponse { ExpiresIn = 3600 }));
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(new MockHttpMessageHandler());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        return client.AsOrOnBehalfOfMerchant(_merchantId);
    }

    [Test]
    public async Task CancelAsync_WhenCancelled_PostsToTheCancelRoute()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.NoContent);

        // Act
        await MerchantWith(apiHandler).CancelAsync(orderId);

        // Assert
        apiHandler.PerformedRequestsCount.ShouldBe(1);
        apiHandler.LastRequestMethod.ShouldBe(HttpMethod.Post);
        apiHandler.LastRequestUri!.AbsolutePath.ShouldEndWith($"/orders/{orderId}/cancel");
    }

    [Test]
    public async Task CancelAsync_WhenTheOrderIsPaid_ThrowsWithTheCodeAndIsNotRetryable()
    {
        // Arrange — the whole point of CancelAsync: a paid order is reported, never refunded.
        var apiHandler = new MockHttpMessageHandler(new ApiErrorResponse
        {
            PropertyName = "Status",
            Message = "The order has been paid and can no longer be cancelled.",
            ErrorCode = CancelOrderErrorCodes.OrderPaid,
        }, HttpStatusCode.BadRequest);

        // Act
        var exception = await Should.ThrowAsync<CancelOrderException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid()));

        // Assert
        exception.ErrorCode.ShouldBe(CancelOrderErrorCodes.OrderPaid);
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.PropertyName.ShouldBe("Status");
        exception.Message.ShouldBe("The order has been paid and can no longer be cancelled.");
        exception.IsRetryable.ShouldBeFalse();
        apiHandler.PerformedRequestsCount.ShouldBe(1);
    }

    [Test]
    public async Task CancelAsync_WhenAPaymentIsInProgress_ThrowsARetryableException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(new ApiErrorResponse
        {
            Message = "A terminal payment attempt is in flight for this order and it cannot be cancelled right now.",
            ErrorCode = CancelOrderErrorCodes.PaymentInProgress,
        }, HttpStatusCode.Conflict);

        // Act
        var exception = await Should.ThrowAsync<CancelOrderException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid()));

        // Assert
        exception.ErrorCode.ShouldBe(CancelOrderErrorCodes.PaymentInProgress);
        exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        exception.IsRetryable.ShouldBeTrue();
    }

    [Test]
    public async Task CancelAsync_WhenTheOrderIsNotFound_ThrowsWithTheNotFoundCode()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(new ApiErrorResponse
        {
            PropertyName = "orderId",
            Message = "The order was not found",
            ErrorCode = CancelOrderErrorCodes.OrderNotFound,
        }, HttpStatusCode.NotFound);

        // Act
        var exception = await Should.ThrowAsync<CancelOrderException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid()));

        // Assert
        exception.ErrorCode.ShouldBe(CancelOrderErrorCodes.OrderNotFound);
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.IsRetryable.ShouldBeFalse();
    }

    [Test]
    public async Task CancelAsync_WhenTheErrorHasNoBody_StillThrowsCancelOrderException()
    {
        // Arrange — e.g. a 404 from an API version that does not have the cancel route yet.
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.NotFound);

        // Act
        var exception = await Should.ThrowAsync<CancelOrderException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid()));

        // Assert
        exception.ErrorCode.ShouldBeNull();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldContain("404");
    }

    [Test]
    public async Task CancelAsync_WhenTheOrderBelongsToAnotherMerchant_ThrowsForbiddenException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(new ErrorResponse
        {
            Error = "merchant_mismatch",
            ErrorDescription = "The order belongs to a different merchant than the API key.",
        }, HttpStatusCode.Forbidden);

        // Act
        var exception = await Should.ThrowAsync<ForbiddenException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid()));

        // Assert
        exception.Error.ShouldBe("merchant_mismatch");
    }

    [Test]
    public async Task CancelAsync_WhenServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.InternalServerError);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task CancelAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.NoContent);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await MerchantWith(apiHandler).CancelAsync(Guid.NewGuid(), cts.Token));
    }
}
