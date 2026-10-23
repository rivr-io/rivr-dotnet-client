using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

/// <summary>
/// <c>RefundAsync</c> and the <c>Idempotency-Key</c> header: the API makes a refund safe to retry when the request
/// carries the same key, so the client must send one, and the same one for every attempt of the same refund.
/// </summary>
public class RefundIdempotencyTests
{
    private const string HeaderName = "Idempotency-Key";

    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    private (IMerchantOperations Merchant, MockHttpMessageHandler Auth) MerchantWith(HttpMessageHandler apiHandler)
    {
        var authHandler = new MockHttpMessageHandler(new TokenResponse { ExpiresIn = 3600 });
        var client = new Client(
            new HttpClient(authHandler),
            new HttpClient(apiHandler),
            new HttpClient(new MockHttpMessageHandler()),
            _config,
            new MemoryCache(new MemoryCacheOptions()));

        return (client.AsOrOnBehalfOfMerchant(_merchantId), authHandler);
    }

    // ---- Without a key from the caller: a generated one, per call ----

    [Test]
    public async Task RefundAsync_WithoutKey_SendsAGuidAsIdempotencyKey()
    {
        var orderId = Guid.NewGuid();
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(orderId);

        api.Attempts.Count.ShouldBe(1);
        var attempt = api.Attempts[0];
        attempt.Method.ShouldBe(HttpMethod.Post);
        attempt.Path.ShouldEndWith($"/orders/{orderId}/refund");
        attempt.BearerTokenSent.ShouldBeTrue();
        attempt.IdempotencyKeys.Length.ShouldBe(1);
        Guid.TryParse(attempt.IdempotencyKeys[0], out var key).ShouldBeTrue();
        key.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public async Task RefundAsync_WithOnlyACancellationToken_StillSendsAGeneratedKey()
    {
        // The signature that existed before the key was added keeps compiling and keeps working.
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), CancellationToken.None);

        api.Attempts.Single().IdempotencyKeys.Single().ShouldNotBeNullOrEmpty();
        Guid.TryParse(api.Attempts.Single().IdempotencyKeys.Single(), out _).ShouldBeTrue();
    }

    [Test]
    public async Task RefundAsync_WithNullKey_GeneratesOne()
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), idempotencyKey: null);

        Guid.TryParse(api.Attempts.Single().IdempotencyKeys.Single(), out _).ShouldBeTrue();
    }

    [Test]
    public async Task RefundAsync_WithoutKey_SendsANewKeyForEveryCall()
    {
        // A second call is a new refund as far as the client can tell. To retry across calls, pass your own key.
        var orderId = Guid.NewGuid();
        var api = new RecordingHandler(HttpStatusCode.NoContent);
        var merchant = MerchantWith(api).Merchant;

        await merchant.RefundAsync(orderId);
        await merchant.RefundAsync(orderId);

        api.Attempts.Count.ShouldBe(2);
        api.Attempts[0].IdempotencyKeys.Single().ShouldNotBe(api.Attempts[1].IdempotencyKeys.Single());
    }

    // ---- With a key from the caller ----

    [Test]
    public async Task RefundAsync_WithKey_SendsThatKey()
    {
        var orderId = Guid.NewGuid();
        var key = Guid.NewGuid().ToString();
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(orderId, key);

        api.Attempts.Count.ShouldBe(1);
        api.Attempts[0].Path.ShouldEndWith($"/orders/{orderId}/refund");
        api.Attempts[0].IdempotencyKeys.ShouldBe(new[] { key });
    }

    [Test]
    public async Task RefundAsync_WithKeyAndCancellationToken_SendsThatKey()
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), "refund-1", CancellationToken.None);

        api.Attempts.Single().IdempotencyKeys.ShouldBe(new[] { "refund-1" });
    }

    [Test]
    public async Task RefundAsync_WithNamedKeyArgument_ResolvesToTheOverloadWithAKey()
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), idempotencyKey: "refund-2");

        api.Attempts.Single().IdempotencyKeys.ShouldBe(new[] { "refund-2" });
    }

    [Test]
    public async Task RefundAsync_WhenTheCallerRetriesWithTheSameKey_SendsTheSameKeyTwice()
    {
        // The documented way to retry across calls: the first attempt fails with a retryable status, the second call
        // reuses the key and the API answers with the stored response.
        var orderId = Guid.NewGuid();
        var api = new RecordingHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.NoContent);
        var merchant = MerchantWith(api).Merchant;
        var key = Guid.NewGuid().ToString();

        await Should.ThrowAsync<RivrHttpRequestException>(async () => await merchant.RefundAsync(orderId, key));
        await merchant.RefundAsync(orderId, key);

        api.Attempts.Count.ShouldBe(2);
        api.Attempts[0].IdempotencyKeys.ShouldBe(new[] { key });
        api.Attempts[1].IdempotencyKeys.ShouldBe(new[] { key });
    }

    [TestCase("a")]
    [TestCase("order 42 refund #1")]
    [TestCase("!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~")]
    public async Task RefundAsync_WithAnyPrintableAsciiKey_SendsItAsOneHeaderValue(string key)
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), key);

        api.Attempts.Single().IdempotencyKeys.ShouldBe(new[] { key });
    }

    [Test]
    public async Task RefundAsync_WithA255CharacterKey_SendsIt()
    {
        var key = new string('k', 255);
        var api = new RecordingHandler(HttpStatusCode.NoContent);

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), key);

        api.Attempts.Single().IdempotencyKeys.ShouldBe(new[] { key });
    }

    // ---- A key the API would refuse never leaves the client ----

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("   ")]
    [TestCase("two\nlines")]
    [TestCase("tab\there")]
    [TestCase("nul\0byte")]
    [TestCase("del\u007fchar")]
    [TestCase("åäö")]
    [TestCase("emoji-😀")]
    public async Task RefundAsync_WithAKeyTheApiWouldRefuse_ThrowsBeforeAnythingIsSent(string key)
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent);
        var (merchant, auth) = MerchantWith(api);

        var exception = await Should.ThrowAsync<ArgumentException>(async () => await merchant.RefundAsync(Guid.NewGuid(), key));

        exception.ParamName.ShouldBe("idempotencyKey");
        api.Attempts.ShouldBeEmpty();
        auth.PerformedRequestsCount.ShouldBe(0);
    }

    [Test]
    public async Task RefundAsync_WithA256CharacterKey_ThrowsAndDoesNotEchoTheKey()
    {
        var key = new string('k', 256);
        var api = new RecordingHandler(HttpStatusCode.NoContent);
        var merchant = MerchantWith(api).Merchant;

        var exception = await Should.ThrowAsync<ArgumentException>(async () => await merchant.RefundAsync(Guid.NewGuid(), key));

        exception.Message.ShouldNotContain(key);
        api.Attempts.ShouldBeEmpty();
    }

    // ---- The pipeline's own retries ----

    [Test]
    public async Task RefundAsync_WhenAHandlerInThePipelineRetries_EveryAttemptCarriesTheSameKey()
    {
        // What a resilience handler added through IHttpClientFactory does: it sends a copy of the request again. The
        // key is a header on the request, set once per call, so the copy carries the same key.
        var recorder = new RecordingHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.NoContent);
        var merchant = MerchantWith(new RetryOnceOnServiceUnavailableHandler(recorder)).Merchant;

        await merchant.RefundAsync(Guid.NewGuid());

        recorder.Attempts.Count.ShouldBe(2);
        recorder.Attempts[0].IdempotencyKeys.Length.ShouldBe(1);
        recorder.Attempts[1].IdempotencyKeys.ShouldBe(recorder.Attempts[0].IdempotencyKeys);
        Guid.TryParse(recorder.Attempts[0].IdempotencyKeys[0], out _).ShouldBeTrue();
    }

    [Test]
    public async Task RefundAsync_WhenAHandlerInThePipelineRetriesWithACallerKey_EveryAttemptCarriesThatKey()
    {
        var recorder = new RecordingHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.NoContent);
        var merchant = MerchantWith(new RetryOnceOnServiceUnavailableHandler(recorder)).Merchant;

        await merchant.RefundAsync(Guid.NewGuid(), "refund-3");

        recorder.Attempts.Count.ShouldBe(2);
        recorder.Attempts.ShouldAllBe(a => a.IdempotencyKeys.Length == 1 && a.IdempotencyKeys[0] == "refund-3");
    }

    // ---- What the API answers does not change what the client does with it ----

    [Test]
    public async Task RefundAsync_WhenTheApiReplaysAStoredResponse_Completes()
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent) { ResponseHeaders = { ["Idempotent-Replayed"] = "true" } };

        await MerchantWith(api).Merchant.RefundAsync(Guid.NewGuid(), "refund-4");

        api.Attempts.Count.ShouldBe(1);
    }

    [TestCase(HttpStatusCode.Conflict, "idempotency_in_progress")]
    [TestCase(HttpStatusCode.UnprocessableEntity, "idempotency_mismatch")]
    [TestCase(HttpStatusCode.ServiceUnavailable, "idempotency_unavailable")]
    [TestCase(HttpStatusCode.InternalServerError, "idempotency_outcome_unknown")]
    public async Task RefundAsync_WhenTheApiAnswersAnIdempotencyError_ThrowsWithTheStatusAndTheBody(HttpStatusCode status, string errorCode)
    {
        var api = new RecordingHandler(status) { Body = $"{{\"propertyName\":\"{HeaderName}\",\"errorCode\":\"{errorCode}\",\"message\":\"x\"}}" };
        var merchant = MerchantWith(api).Merchant;

        var exception = await Should.ThrowAsync<RivrHttpRequestException>(async () => await merchant.RefundAsync(Guid.NewGuid(), "refund-5"));

        exception.StatusCode.ShouldBe(status);
        exception.ResponseContent.ShouldNotBeNull().ShouldContain(errorCode);
    }

    [Test]
    public async Task RefundAsync_WhenTheApiRefusesTheKey_ThrowsApiCallExceptionLikeAnyOtherBadRequest()
    {
        var api = new RecordingHandler(HttpStatusCode.BadRequest)
        {
            Body = $"{{\"propertyName\":\"{HeaderName}\",\"errorCode\":\"idempotency_key_invalid\",\"message\":\"The key is not valid.\"}}",
        };
        var merchant = MerchantWith(api).Merchant;

        var exception = await Should.ThrowAsync<ApiCallException>(async () => await merchant.RefundAsync(Guid.NewGuid(), "refund-6"));

        exception.Message.ShouldBe("The key is not valid.");
    }

    [Test]
    public async Task RefundAsync_WhenCancellationIsRequested_ThrowsWithoutSending()
    {
        var api = new RecordingHandler(HttpStatusCode.NoContent);
        var merchant = MerchantWith(api).Merchant;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () => await merchant.RefundAsync(Guid.NewGuid(), "refund-7", cts.Token));

        api.Attempts.ShouldBeEmpty();
    }

    // ---- Test doubles ----

    private sealed record Attempt(HttpMethod Method, string Path, string[] IdempotencyKeys, bool BearerTokenSent);

    /// <summary>Answers with the given statuses in turn (the last one repeats) and records what it was sent.</summary>
    private sealed class RecordingHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private int _next;

        public List<Attempt> Attempts { get; } = new();

        public string Body { get; init; } = string.Empty;

        public Dictionary<string, string> ResponseHeaders { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var keys = request.Headers.TryGetValues(HeaderName, out var values) ? values.ToArray() : [];
            Attempts.Add(new Attempt(request.Method, request.RequestUri!.AbsolutePath, keys, request.Headers.Authorization is { Scheme: "Bearer" }));

            var response = new HttpResponseMessage(statuses[Math.Min(_next++, statuses.Length - 1)]) { Content = new StringContent(Body) };
            foreach (var (name, value) in ResponseHeaders)
            {
                response.Headers.Add(name, value);
            }

            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Stands in for a resilience handler: on 503 it sends a copy of the request once more. Like Polly's, the copy is
    /// made from the request as it was sent, headers included.
    /// </summary>
    private sealed class RetryOnceOnServiceUnavailableHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var first = await base.SendAsync(request, cancellationToken);
            if (first.StatusCode != HttpStatusCode.ServiceUnavailable)
            {
                return first;
            }

            first.Dispose();
            return await base.SendAsync(Copy(request), cancellationToken);
        }

        private static HttpRequestMessage Copy(HttpRequestMessage request)
        {
            var copy = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            copy.Headers.Authorization = request.Headers.Authorization is { } authorization
                ? new AuthenticationHeaderValue(authorization.Scheme, authorization.Parameter)
                : null;

            return copy;
        }
    }
}
