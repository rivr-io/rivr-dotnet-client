using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class SendOrderToDeviceTests
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
    public async Task SendOrderToDeviceAsync_WhenAccepted_PostsTheDeviceToTheSendToDeviceRoute()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.Accepted);

        // Act
        await MerchantWith(apiHandler).SendOrderToDeviceAsync(orderId, deviceId);

        // Assert
        apiHandler.PerformedRequestsCount.ShouldBe(1);
        apiHandler.LastRequestMethod.ShouldBe(HttpMethod.Post);
        apiHandler.LastRequestUri!.AbsolutePath.ShouldEndWith($"/orders/{orderId}/send-to-device");
        apiHandler.GetRequestContent<SendOrderToDeviceRequest>()!.DeviceId.ShouldBe(deviceId);
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenTheOrderIsNotCreated_ThrowsAndIsNotRetryable()
    {
        // Arrange — a paid or cancelled order must never reach a terminal.
        var apiHandler = new MockHttpMessageHandler(new ApiErrorResponse
        {
            PropertyName = "Status",
            Message = "The order is in status Completed and can therefore not be sent to a device",
        }, HttpStatusCode.BadRequest);

        // Act
        var exception = await Should.ThrowAsync<SendOrderToDeviceException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.PropertyName.ShouldBe("Status");
        exception.Message.ShouldBe("The order is in status Completed and can therefore not be sent to a device");
        exception.IsRetryable.ShouldBeFalse();
        apiHandler.PerformedRequestsCount.ShouldBe(1);
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenTheDeviceIsOffline_ThrowsARetryableException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(new ApiErrorResponse
        {
            PropertyName = "DeviceId",
            Message = "The device is offline and cannot receive the order right now. Retry when it is online.",
        }, HttpStatusCode.Conflict);

        // Act
        var exception = await Should.ThrowAsync<SendOrderToDeviceException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        exception.PropertyName.ShouldBe("DeviceId");
        exception.IsRetryable.ShouldBeTrue();
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenTheOrderIsNotFound_Throws()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(new ApiErrorResponse
        {
            PropertyName = "orderId",
            Message = "The order was not found",
        }, HttpStatusCode.NotFound);

        // Act
        var exception = await Should.ThrowAsync<SendOrderToDeviceException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.PropertyName.ShouldBe("orderId");
        exception.IsRetryable.ShouldBeFalse();
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenTheErrorHasNoBody_StillThrowsSendOrderToDeviceException()
    {
        // Arrange — e.g. a 404 from an API version that does not have the send-to-device route.
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.NotFound);

        // Act
        var exception = await Should.ThrowAsync<SendOrderToDeviceException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.PropertyName.ShouldBeNull();
        exception.Message.ShouldContain("404");
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenTheOrderBelongsToAnotherMerchant_ThrowsForbiddenException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(new ErrorResponse
        {
            Error = "merchant_mismatch",
            ErrorDescription = "The order belongs to a different merchant than the API key.",
        }, HttpStatusCode.Forbidden);

        // Act
        var exception = await Should.ThrowAsync<ForbiddenException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        exception.Error.ShouldBe("merchant_mismatch");
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.InternalServerError);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Test]
    public async Task SendOrderToDeviceAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.Accepted);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await MerchantWith(apiHandler).SendOrderToDeviceAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token));
    }
}
