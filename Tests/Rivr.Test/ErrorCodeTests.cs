using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Extensions;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

/// <summary>
/// Every error answer under <c>/api/public</c> has carried a stable <c>code</c> and a <c>type</c> since 2026-10-25, and
/// every answer from rivr-public-api has the header <c>Rivr-Request-Id</c>. The exceptions of the client expose all
/// three, next to what they have always exposed. The bodies below have the shapes of the developer portal's error
/// catalog (<c>problemBody</c>, form <c>errorObject</c> and <c>problem</c>), with made-up ids.
/// </summary>
public class ErrorCodeTests
{
    private const string RequestId = "5f0c2a7e-9b41-4d3a-8e6f-1c2d3b4a5e6f";
    private const string TypeBase = "https://developers.rivr.io/errors/";
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _orderId = Guid.Parse("1f3e2b7a-5c0d-4d8e-9a41-2b6f0c9d7e15");

    private sealed class RawHandler(HttpStatusCode status, string body, params (string Name, string Value)[] headers) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            };
            foreach (var (name, value) in headers)
            {
                response.Headers.TryAddWithoutValidation(name, value);
            }

            return Task.FromResult(response);
        }
    }

    private IMerchantOperations MerchantAnswering(HttpStatusCode status, string body, params (string Name, string Value)[] headers)
    {
        var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
        var authHttpClient = new HttpClient(new MockHttpMessageHandler(new TokenResponse { ExpiresIn = 3600 }));
        var apiHttpClient = new HttpClient(new RawHandler(status, body, headers));
        var webhookHttpClient = new HttpClient(new MockHttpMessageHandler());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, config, new MemoryCache(new MemoryCacheOptions()));
        return client.AsOrOnBehalfOfMerchant(_merchantId);
    }

    // The error object of an order operation: propertyName, message, errorCode, and code and type since 2026-10-25.
    private static string ErrorObject(string code, string message, string? propertyName = null) =>
        $$"""
        {"propertyName":{{(propertyName is null ? "null" : $"\"{propertyName}\"")}},"message":"{{message}}","errorCode":"{{code}}","code":"{{code}}","type":"{{TypeBase}}{{code}}"}
        """;

    [Test]
    public async Task CancelAsync_exposes_code_type_and_request_id_next_to_what_it_always_did()
    {
        var body = ErrorObject("order_paid", "The order has been paid and can no longer be cancelled.", "Status");

        var exception = await Should.ThrowAsync<CancelOrderException>(async () =>
            await MerchantAnswering(HttpStatusCode.BadRequest, body, ("Rivr-Request-Id", RequestId)).CancelAsync(_orderId));

        exception.Code.ShouldBe("order_paid");
        exception.Type.ShouldBe(TypeBase + "order_paid");
        exception.RequestId.ShouldBe(RequestId);
        // What was there before is as it was.
        exception.ErrorCode.ShouldBe("order_paid");
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.PropertyName.ShouldBe("Status");
        exception.Message.ShouldBe("The order has been paid and can no longer be cancelled.");
        exception.IsRetryable.ShouldBeFalse();
    }

    [Test]
    public async Task CancelAsync_without_code_and_header_has_null_for_all_three()
    {
        // An API version from before 2026-10-25, or a gateway that does not add the header.
        const string body = "{\"propertyName\":\"Status\",\"message\":\"Not cancellable.\",\"errorCode\":\"order_not_cancellable\"}";

        var exception = await Should.ThrowAsync<CancelOrderException>(async () =>
            await MerchantAnswering(HttpStatusCode.BadRequest, body).CancelAsync(_orderId));

        exception.ErrorCode.ShouldBe("order_not_cancellable");
        exception.Code.ShouldBeNull();
        exception.Type.ShouldBeNull();
        exception.RequestId.ShouldBeNull();
    }

    [Test]
    public async Task SendOrderToDeviceAsync_exposes_code_type_and_request_id()
    {
        var body = ErrorObject("device_offline", "The device is offline.", "DeviceId");

        var exception = await Should.ThrowAsync<SendOrderToDeviceException>(async () =>
            await MerchantAnswering(HttpStatusCode.Conflict, body, ("Rivr-Request-Id", RequestId))
                .SendOrderToDeviceAsync(_orderId, Guid.NewGuid()));

        exception.Code.ShouldBe("device_offline");
        exception.Type.ShouldBe(TypeBase + "device_offline");
        exception.RequestId.ShouldBe(RequestId);
        exception.IsRetryable.ShouldBeTrue();
        exception.PropertyName.ShouldBe("DeviceId");
        exception.Message.ShouldBe("The device is offline.");
    }

    [Test]
    public async Task RefundAsync_400_is_an_ApiCallException_that_exposes_code_type_and_request_id()
    {
        var body = ErrorObject("order_not_refundable", "The order cannot be refunded.", "Status");

        var exception = await Should.ThrowAsync<ApiCallException>(async () =>
            await MerchantAnswering(HttpStatusCode.BadRequest, body, ("Rivr-Request-Id", RequestId)).RefundAsync(_orderId));

        exception.GetType().ShouldBe(typeof(ApiCallException));
        exception.Code.ShouldBe("order_not_refundable");
        exception.Type.ShouldBe(TypeBase + "order_not_refundable");
        exception.RequestId.ShouldBe(RequestId);
        exception.Message.ShouldBe("The order cannot be refunded.");
    }

    [Test]
    public async Task A_401_is_an_UnauthorizedException_that_exposes_code_type_and_request_id_and_keeps_its_message()
    {
        // The members the answer carried before it got a code come first, then the error object.
        var body = "{\"title\":\"Unauthorized\",\"status\":401,\"detail\":\"The token is invalid.\","
                   + ErrorObject("token_expired", "The token has expired. Get a new token and retry the request.").TrimStart('{');

        var exception = await Should.ThrowAsync<UnauthorizedException>(async () =>
            await MerchantAnswering(HttpStatusCode.Unauthorized, body, ("Rivr-Request-Id", RequestId)).GetOrderAsync(_orderId));

        exception.Code.ShouldBe("token_expired");
        exception.Type.ShouldBe(TypeBase + "token_expired");
        exception.RequestId.ShouldBe(RequestId);
        exception.Message.ShouldBe("Unauthorized. Please check your credentials.");
    }

    [Test]
    public async Task A_401_without_a_body_is_still_an_UnauthorizedException_with_the_request_id_from_the_header()
    {
        var exception = await Should.ThrowAsync<UnauthorizedException>(async () =>
            await MerchantAnswering(HttpStatusCode.Unauthorized, string.Empty, ("Rivr-Request-Id", RequestId)).GetOrderAsync(_orderId));

        exception.Code.ShouldBeNull();
        exception.Type.ShouldBeNull();
        exception.RequestId.ShouldBe(RequestId);
        exception.Message.ShouldBe("Unauthorized. Please check your credentials.");
    }

    [Test]
    public async Task A_403_is_a_ForbiddenException_that_exposes_code_type_and_request_id_next_to_error_and_description()
    {
        // The scope refusal keeps error and error_description, and code has the same value as error.
        const string description = "The scope(s) 'public:orders:write' is required.";
        var body = "{\"error\":\"invalid_scope\",\"error_description\":\"" + description + "\","
                   + ErrorObject("invalid_scope", description).TrimStart('{');

        var exception = await Should.ThrowAsync<ForbiddenException>(async () =>
            await MerchantAnswering(HttpStatusCode.Forbidden, body, ("Rivr-Request-Id", RequestId)).GetOrderAsync(_orderId));

        exception.Error.ShouldBe("invalid_scope");
        exception.ErrorDescription.ShouldBe(description);
        exception.Message.ShouldBe(description);
        exception.Code.ShouldBe("invalid_scope");
        exception.Type.ShouldBe(TypeBase + "invalid_scope");
        exception.RequestId.ShouldBe(RequestId);
    }

    [Test]
    public async Task A_403_in_the_old_shape_has_null_for_code_and_type()
    {
        const string body = "{\"error\":\"forbidden\",\"error_description\":\"Access denied to this resource\"}";

        var exception = await Should.ThrowAsync<ForbiddenException>(async () =>
            await MerchantAnswering(HttpStatusCode.Forbidden, body).GetOrderAsync(_orderId));

        exception.Error.ShouldBe("forbidden");
        exception.Code.ShouldBeNull();
        exception.Type.ShouldBeNull();
        exception.RequestId.ShouldBeNull();
    }

    [Test]
    public async Task Any_other_status_is_a_RivrHttpRequestException_that_exposes_code_type_and_request_id_and_keeps_its_message()
    {
        var body = ErrorObject("order_not_found", "Order not found.", "orderId");

        var exception = await Should.ThrowAsync<RivrHttpRequestException>(async () =>
            await MerchantAnswering(HttpStatusCode.NotFound, body, ("Rivr-Request-Id", RequestId)).GetOrderAsync(_orderId));

        exception.Code.ShouldBe("order_not_found");
        exception.Type.ShouldBe(TypeBase + "order_not_found");
        exception.RequestId.ShouldBe(RequestId);
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.ResponseContent.ShouldBe(body);
        // The message is as it was: no body, no code. (The request id has its own property, since the correlation id in
        // the message is another thing.)
        exception.Message.ShouldStartWith("Response status code does not indicate success: 404 (");
        exception.Message.ShouldNotContain("order_not_found");
        exception.Message.ShouldNotContain(RequestId);
    }

    [Test]
    public async Task A_validation_problem_from_the_framework_has_its_code_in_code_not_in_errorCode()
    {
        // form "problem": ProblemDetails with code and message added and type pointing at the error's page.
        var body = $$"""
            {"type":"{{TypeBase}}invalid_request_body","title":"One or more validation errors occurred.","status":400,"errors":{"order":["The order field is required."]},"traceId":"00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00","code":"invalid_request_body","message":"The request body could not be read."}
            """;

        var exception = await Should.ThrowAsync<RivrHttpRequestException>(async () =>
            await MerchantAnswering(HttpStatusCode.BadRequest, body, ("Rivr-Request-Id", RequestId)).GetOrderAsync(_orderId));

        exception.Code.ShouldBe("invalid_request_body");
        exception.Type.ShouldBe(TypeBase + "invalid_request_body");
        exception.RequestId.ShouldBe(RequestId);
    }

    [Test]
    public async Task A_body_that_is_not_an_error_object_gives_null_and_the_exception_is_still_thrown()
    {
        var exception = await Should.ThrowAsync<RivrHttpRequestException>(async () =>
            await MerchantAnswering(HttpStatusCode.BadGateway, "<html><body>Bad gateway</body></html>").GetOrderAsync(_orderId));

        exception.Code.ShouldBeNull();
        exception.Type.ShouldBeNull();
        exception.RequestId.ShouldBeNull();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
        exception.ResponseContent.ShouldBe("<html><body>Bad gateway</body></html>");
    }

    [TestCase("{\"code\":\"order_paid\",\"type\":\"https://developers.rivr.io/errors/order_paid\"}", "order_paid", "https://developers.rivr.io/errors/order_paid")]
    [TestCase("{\"Code\":\"order_paid\",\"Type\":\"https://developers.rivr.io/errors/order_paid\"}", "order_paid", "https://developers.rivr.io/errors/order_paid")]
    [TestCase("{\"errorCode\":\"order_paid\"}", null, null)]                 // errorCode is not code
    [TestCase("{\"code\":123,\"type\":true}", null, null)]                    // not strings
    [TestCase("{\"code\":null,\"type\":null}", null, null)]
    [TestCase("{\"code\":\"has a space\",\"type\":\"has a space\"}", null, null)]
    [TestCase("{\"code\":\"line\\nbreak\",\"type\":\"line\\nbreak\"}", null, null)]
    [TestCase("{\"code\":\"\",\"type\":\"\"}", null, null)]
    [TestCase("{\"code\":\"återbesök\",\"type\":\"https://example.com/å\"}", null, null)]   // not ASCII
    [TestCase("[]", null, null)]
    [TestCase("\"order_paid\"", null, null)]
    [TestCase("null", null, null)]
    [TestCase("{\"code\":", null, null)]                                       // cut off
    [TestCase("not json", null, null)]
    [TestCase("", null, null)]
    [TestCase("   ", null, null)]
    [TestCase(null, null, null)]
    public void ReadBody_takes_code_and_type_only_when_they_are_strings_shaped_like_the_APIs(string? body, string? code, string? type)
    {
        var (actualCode, actualType) = ApiErrorFields.ReadBody(body);

        actualCode.ShouldBe(code);
        actualType.ShouldBe(type);
    }

    [Test]
    public void ReadBody_drops_a_code_longer_than_64_characters_and_a_type_longer_than_256()
    {
        var (code64, _) = ApiErrorFields.ReadBody($"{{\"code\":\"{new string('a', 64)}\"}}");
        var (code65, _) = ApiErrorFields.ReadBody($"{{\"code\":\"{new string('a', 65)}\"}}");
        var (_, type256) = ApiErrorFields.ReadBody($"{{\"type\":\"{new string('a', 256)}\"}}");
        var (_, type257) = ApiErrorFields.ReadBody($"{{\"type\":\"{new string('a', 257)}\"}}");

        code64.ShouldNotBeNull();
        code65.ShouldBeNull();
        type256.ShouldNotBeNull();
        type257.ShouldBeNull();
    }

    [Test]
    public void ReadBody_does_not_throw_for_a_body_nested_deeper_than_the_parser_allows()
    {
        var body = new string('[', 5000) + new string(']', 5000);

        ApiErrorFields.ReadBody(body).ShouldBe((null, null));
    }

    [TestCase(RequestId, RequestId)]
    [TestCase("  " + RequestId + "  ", RequestId)]
    [TestCase("not-a-guid-but-printable", "not-a-guid-but-printable")]   // the id may change shape; it is only kept clean
    [TestCase("has a space", null)]
    [TestCase("", null)]
    [TestCase("å", null)]
    public void RequestIdOf_keeps_a_clean_header_value_and_drops_the_rest(string value, string? expected)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        response.Headers.TryAddWithoutValidation("Rivr-Request-Id", value);

        ApiErrorFields.RequestIdOf(response).ShouldBe(expected);
    }

    [Test]
    public void RequestIdOf_drops_a_value_longer_than_128_characters_and_answers_null_without_the_header()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        ApiErrorFields.RequestIdOf(response).ShouldBeNull();

        response.Headers.TryAddWithoutValidation("rivr-request-id", new string('a', 128));
        ApiErrorFields.RequestIdOf(response).ShouldNotBeNull();

        using var tooLong = new HttpResponseMessage(HttpStatusCode.NotFound);
        tooLong.Headers.TryAddWithoutValidation("Rivr-Request-Id", new string('a', 129));
        ApiErrorFields.RequestIdOf(tooLong).ShouldBeNull();
    }

    [Test]
    public async Task EnsureSuccessfulResponseAsync_drops_what_is_not_shaped_like_the_APIs_values_on_all_three_exceptions()
    {
        // A proxy's JSON error page, say: free text where the API would have a short code.
        const string body = "{\"code\":\"Something went wrong\\nPlease retry\",\"type\":\"free text\",\"error\":\"x\",\"error_description\":\"y\"}";
        HttpResponseMessage Failed(HttpStatusCode status) => new(status)
        {
            Content = new StringContent(body),
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.rivr.io/api/public/orders/{_orderId}"),
        };

        var unauthorized = await Should.ThrowAsync<UnauthorizedException>(() => Failed(HttpStatusCode.Unauthorized).EnsureSuccessfulResponseAsync());
        var forbidden = await Should.ThrowAsync<ForbiddenException>(() => Failed(HttpStatusCode.Forbidden).EnsureSuccessfulResponseAsync());
        var other = await Should.ThrowAsync<RivrHttpRequestException>(() => Failed(HttpStatusCode.Conflict).EnsureSuccessfulResponseAsync());

        foreach (var (code, type) in new[] { (unauthorized.Code, unauthorized.Type), (forbidden.Code, forbidden.Type), (other.Code, other.Type) })
        {
            code.ShouldBeNull();
            type.ShouldBeNull();
        }

        forbidden.Error.ShouldBe("x");
    }

    [Test]
    public void The_constructors_that_were_there_before_still_work_and_leave_the_new_properties_null()
    {
        var api = new ApiCallException("message");
        var apiWithInner = new ApiCallException("message", new InvalidOperationException());
        var unauthorized = new UnauthorizedException("message");
        var forbidden = new ForbiddenException("error", "description");
        var forbiddenFromResponse = new ForbiddenException(new ErrorResponse { Error = "e", ErrorDescription = "d" });
        var cancel = new CancelOrderException(HttpStatusCode.Conflict, new ApiErrorResponse { Message = "m", ErrorCode = "payment_in_progress" });
        var send = new SendOrderToDeviceException(HttpStatusCode.Conflict, new ApiErrorResponse { Message = "m" });
        var http = new RivrHttpRequestException("message", HttpStatusCode.BadRequest, "Bad Request", "GET", "/path", null, "{}");

        (api.Code, api.Type, api.RequestId).ShouldBe((null, null, null));
        (apiWithInner.Code, apiWithInner.Type, apiWithInner.RequestId).ShouldBe((null, null, null));
        (unauthorized.Code, unauthorized.Type, unauthorized.RequestId).ShouldBe((null, null, null));
        (forbidden.Code, forbidden.Type, forbidden.RequestId).ShouldBe((null, null, null));
        (forbiddenFromResponse.Code, forbiddenFromResponse.Type, forbiddenFromResponse.RequestId).ShouldBe((null, null, null));
        (cancel.Code, cancel.Type, cancel.RequestId).ShouldBe((null, null, null));
        (send.Code, send.Type, send.RequestId).ShouldBe((null, null, null));
        (http.Code, http.Type, http.RequestId).ShouldBe((null, null, null));
        cancel.ErrorCode.ShouldBe("payment_in_progress");
        cancel.IsRetryable.ShouldBeTrue();
    }

    [Test]
    public void The_exceptions_can_be_made_with_the_three_values_for_a_test_of_your_own()
    {
        var api = new ApiCallException("m", "order_paid", TypeBase + "order_paid", RequestId);
        var unauthorized = new UnauthorizedException("m", "token_expired", TypeBase + "token_expired", RequestId);
        var forbidden = new ForbiddenException(new ErrorResponse { Error = "e", ErrorDescription = "d", Code = "invalid_scope", Type = TypeBase + "invalid_scope" }, RequestId);
        var http = new RivrHttpRequestException("m", HttpStatusCode.NotFound, "Not Found", "GET", "/p", null, "{}", "order_not_found", TypeBase + "order_not_found", RequestId);

        api.Code.ShouldBe("order_paid");
        api.RequestId.ShouldBe(RequestId);
        unauthorized.Code.ShouldBe("token_expired");
        forbidden.Code.ShouldBe("invalid_scope");
        forbidden.Type.ShouldBe(TypeBase + "invalid_scope");
        forbidden.RequestId.ShouldBe(RequestId);
        http.Code.ShouldBe("order_not_found");
        http.RequestId.ShouldBe(RequestId);
    }
}
