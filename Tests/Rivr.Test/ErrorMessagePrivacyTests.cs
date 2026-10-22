using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Rivr.Extensions;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

/// <summary>
/// Exception messages end up in the integrator's logs and error tracker. Request and response bodies can carry a
/// customer's personal identity number, so they must never be part of a message. The personal data below is made
/// up: 19121212-1212 is the Swedish Tax Agency's well-known test person, and example.com is reserved for examples.
/// </summary>
public class ErrorMessagePrivacyTests
{
    private const string TestPersonalNumber = "191212121212";
    private static readonly Guid OrderId = Guid.Parse("1f3e2b7a-5c0d-4d8e-9a41-2b6f0c9d7e15");

    private static HttpResponseMessage FailedResponse(
        HttpStatusCode statusCode,
        string reasonPhrase,
        HttpMethod method,
        string url,
        string? requestBody = null,
        string? responseBody = null) =>
        new(statusCode)
        {
            ReasonPhrase = reasonPhrase,
            Content = new StringContent(responseBody ?? string.Empty),
            RequestMessage = new HttpRequestMessage(method, url)
            {
                Content = requestBody is null ? null : new StringContent(requestBody)
            }
        };

    [Test]
    public async Task EnsureSuccessfulResponseAsync_MessageCarriesStatusMethodAndPath_ButNoBodies()
    {
        // Arrange — an order with the customer's personal number, rejected with the number echoed back.
        var response = FailedResponse(
            HttpStatusCode.BadRequest,
            "Bad Request",
            HttpMethod.Post,
            "https://api.rivr.io/api/public/orders?merchant=abc",
            requestBody: $"{{\"customer\":{{\"personalNumber\":\"{TestPersonalNumber}\"}}}}",
            responseBody: $"{{\"message\":\"Invalid personal number {TestPersonalNumber}\"}}");

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(() => response.EnsureSuccessfulResponseAsync());

        // Assert
        exception.Message.ShouldBe(
            "Response status code does not indicate success: 400 (Bad Request). POST https://api.rivr.io/api/public/orders.");
        exception.Message.ShouldNotContain(TestPersonalNumber);
        exception.ToString().ShouldNotContain(TestPersonalNumber);
        exception.Message.ShouldNotContain("merchant=abc");
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Method.ShouldBe("POST");
        exception.RequestPath.ShouldBe("/api/public/orders");
        exception.ReasonPhrase.ShouldBe("Bad Request");
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_KeepsTheResponseBodyAsAProperty()
    {
        // Arrange
        const string body = "{\"message\":\"Order cannot be refunded\"}";
        var response = FailedResponse(HttpStatusCode.Conflict, "Conflict", HttpMethod.Post,
            $"https://api.rivr.io/api/public/orders/{OrderId}/refund", responseBody: body);

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(() => response.EnsureSuccessfulResponseAsync());

        // Assert
        exception.ResponseContent.ShouldBe(body);
        exception.Message.ShouldNotContain("cannot be refunded");
        exception.RequestPath.ShouldBe($"/api/public/orders/{OrderId}/refund");
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_IsStillAnHttpRequestException()
    {
        // Arrange
        var response = FailedResponse(HttpStatusCode.InternalServerError, "Internal Server Error", HttpMethod.Get,
            "https://api.rivr.io/api/public/health-secure");

        // Act & Assert — existing catch (HttpRequestException) blocks keep working.
        await Should.ThrowAsync<HttpRequestException>(() => response.EnsureSuccessfulResponseAsync());
    }

    [TestCase("https://api.rivr.io/api/public/customers/by-personal-number/191212121212", "/api/public/customers/by-personal-number/{value}")]
    [TestCase("https://api.rivr.io/api/public/customers/19121212-1212/orders", "/api/public/customers/{value}/orders")]
    [TestCase("https://api.rivr.io/api/public/customers/test.person%40example.com", "/api/public/customers/{value}")]
    [TestCase("https://api.rivr.io/api/public/customers/+46701740605", "/api/public/customers/{value}")]
    [TestCase("https://api.rivr.io/api/public/orders/1f3e2b7a-5c0d-4d8e-9a41-2b6f0c9d7e15", "/api/public/orders/1f3e2b7a-5c0d-4d8e-9a41-2b6f0c9d7e15")]
    [TestCase("https://api.rivr.io/api/public/v1/order-settlements", "/api/public/v1/order-settlements")]
    public void RedactPath_MasksEverySegmentThatCouldCarryAValue(string url, string expected)
    {
        HttpResponseMessageExtensions.RedactPath(new Uri(url)).ShouldBe(expected);
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_CarriesAWellFormedCorrelationId()
    {
        // Arrange
        const string correlationId = "4bf92f35-77b3-4da6-a3ce-929d0e0e4736";
        var response = FailedResponse(HttpStatusCode.BadGateway, "Bad Gateway", HttpMethod.Get,
            "https://api.rivr.io/api/public/devices");
        response.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(() => response.EnsureSuccessfulResponseAsync());

        // Assert
        exception.CorrelationId.ShouldBe(correlationId);
        exception.Message.ShouldEndWith($"Correlation id: {correlationId}.");
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_RejectsACorrelationIdThatIsNotAnId()
    {
        // Arrange — the header is set by the caller and can carry anything.
        var response = FailedResponse(HttpStatusCode.BadRequest, "Bad Request", HttpMethod.Get,
            "https://api.rivr.io/api/public/devices");
        response.RequestMessage!.Headers.Add("X-Correlation-Id", TestPersonalNumber);

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(() => response.EnsureSuccessfulResponseAsync());

        // Assert
        exception.Message.ShouldNotContain(TestPersonalNumber);
        exception.CorrelationId.ShouldNotBe(TestPersonalNumber);
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_FallsBackToTheActivityTraceId()
    {
        // Arrange
        using var activity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();
        var response = FailedResponse(HttpStatusCode.ServiceUnavailable, "Service Unavailable", HttpMethod.Get,
            "https://api.rivr.io/api/public/devices");

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(() => response.EnsureSuccessfulResponseAsync());

        // Assert
        exception.CorrelationId.ShouldBe(activity.TraceId.ToHexString());
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_WithoutRequestMessage_StillDescribesTheStatus()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            ReasonPhrase = "Not Found",
            Content = new StringContent(TestPersonalNumber)
        };

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(() => response.EnsureSuccessfulResponseAsync());

        // Assert
        exception.Message.ShouldBe("Response status code does not indicate success: 404 (Not Found).");
        exception.RequestPath.ShouldBeNull();
    }

    [Test]
    public async Task HttpContentDeserialiseAsync_ErrorCarriesNoContent()
    {
        // Arrange
        var content = new StringContent($"{{\"id\":\"not-a-guid\",\"personalNumber\":\"{TestPersonalNumber}\"}}");

        // Act
        var exception = await Should.ThrowAsync<SerializationException>(() => content.DeserialiseAsync<Order>());

        // Assert
        exception.ToString().ShouldNotContain(TestPersonalNumber);
        exception.Message.ShouldBe("Could not deserialize the response into Order.");
        exception.InnerException.ShouldNotBeNull();
    }

    [Test]
    public void StringDeserialise_ErrorCarriesNoContent()
    {
        // Arrange — the shape of a callback's Data.
        var data = $"{{\"createdDate\":\"not-a-date\",\"metadata\":{{\"patient\":\"{TestPersonalNumber}\"}}}}";

        // Act
        var exception = Should.Throw<SerializationException>(() => data.Deserialise<Core.Models.Callbacks.OrderCreated>());

        // Assert
        exception.ToString().ShouldNotContain(TestPersonalNumber);
        exception.Message.ShouldBe("Could not deserialize the content into OrderCreated.");
    }

    [Test]
    public async Task ApiCall_WhenNotFound_ThrowsRivrHttpRequestExceptionWithStatusCode()
    {
        // Arrange
        var authHandler = new MockHttpMessageHandler(new TokenResponse { ExpiresIn = 3600 });
        var apiHandler = new MockHttpMessageHandler(null, HttpStatusCode.NotFound);
        var client = new Client(
            new HttpClient(authHandler),
            new HttpClient(apiHandler),
            new HttpClient(new MockHttpMessageHandler()),
            new Config(clientId: "clientId", clientSecret: "clientSecret"),
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(Guid.Parse("11111111-1111-1111-1111-111111111111"))
                .GetOrderAsync(OrderId));

        // Assert
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldStartWith("Response status code does not indicate success: 404");
    }
}
