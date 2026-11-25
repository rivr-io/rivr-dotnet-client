using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Core.Models;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

/// <summary>
/// The webhook aggregator requires the merchant's access token on every bundle call (401 without a token, 403 for
/// another merchant's token). These tests pin that the client sends it.
/// </summary>
public class WebhookAggregatorTests
{
    private const string AccessToken = "merchant-access-token";
    private static readonly Guid MerchantId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid BundleId = Guid.Parse("33333333-3333-4333-8333-333333333333");

    private sealed class RecordingHandler(HttpStatusCode statusCode, object? body = null) : HttpMessageHandler
    {
        public List<(HttpMethod Method, Uri? Uri, AuthenticationHeaderValue? Authorization)> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.Method, request.RequestUri, request.Headers.Authorization));
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body is null ? string.Empty : JsonSerializer.Serialize(body))
            });
        }
    }

    private static (Client Client, MockHttpMessageHandler Auth, RecordingHandler Aggregator) Create(
        HttpStatusCode aggregatorStatus,
        object? aggregatorBody = null,
        string? accessToken = AccessToken,
        Config? config = null)
    {
        var auth = new MockHttpMessageHandler(new TokenResponse { AccessToken = accessToken, ExpiresIn = 3600 });
        var aggregator = new RecordingHandler(aggregatorStatus, aggregatorBody);
        var client = new Client(
            new HttpClient(auth),
            new HttpClient(new MockHttpMessageHandler()),
            new HttpClient(aggregator),
            config ?? new Config(clientId: "clientId", clientSecret: "clientSecret"),
            Substitute.For<IMemoryCache>());
        return (client, auth, aggregator);
    }

    private static void ShouldCarryTheMerchantToken(RecordingHandler aggregator, HttpMethod method, string pathSuffix)
    {
        aggregator.Requests.Count.ShouldBe(1);
        var request = aggregator.Requests[0];
        request.Method.ShouldBe(method);
        request.Uri!.AbsolutePath.ShouldBe($"/merchants/{MerchantId}/webhook-bundles/{pathSuffix}");
        request.Authorization.ShouldNotBeNull();
        request.Authorization.Scheme.ShouldBe("Bearer");
        request.Authorization.Parameter.ShouldBe(AccessToken);
    }

    [Test]
    public async Task CreateBundle_SendsTheMerchantToken()
    {
        var (client, auth, aggregator) = Create(HttpStatusCode.NoContent);

        await client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.CreateBundleAsync();

        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Post, "");
        auth.RequestContent.ShouldNotBeNull().ShouldContain("\"merchant_token\"");
    }

    [Test]
    public async Task GetNextUnreadBundle_SendsTheMerchantToken_AndReturnsNullOn204()
    {
        var (client, _, aggregator) = Create(HttpStatusCode.NoContent);

        var bundle = await client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.GetNextUnreadBundleAsync();

        bundle.ShouldBeNull();
        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Get, "unread");
    }

    [Test]
    public async Task GetBundle_SendsTheMerchantToken_AndReturnsNullOn204()
    {
        var (client, _, aggregator) = Create(HttpStatusCode.NoContent);

        var bundle = await client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.GetBundleAsync(BundleId);

        bundle.ShouldBeNull();
        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Get, BundleId.ToString());
    }

    [Test]
    public async Task GetBundle_ReturnsTheBundle()
    {
        var (client, _, aggregator) = Create(HttpStatusCode.OK, new { bundleId = BundleId, webhooks = Array.Empty<object>() });

        var bundle = await client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.GetBundleAsync(BundleId);

        bundle.ShouldNotBeNull();
        bundle.BundleId.ShouldBe(BundleId);
        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Get, BundleId.ToString());
    }

    [Test]
    public async Task MarkBundleAsRead_SendsTheMerchantToken()
    {
        var (client, _, aggregator) = Create(HttpStatusCode.NoContent);

        await client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.MarkBundleAsReadAsync(BundleId);

        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Put, BundleId.ToString());
    }

    [Test]
    public async Task MerchantCredentials_SendTheMerchantToken()
    {
        var config = new Config(clientId: "", clientSecret: "", merchantId: MerchantId.ToString(), merchantSecret: "merchantSecret");

        var (client, auth, aggregator) = Create(HttpStatusCode.NoContent, config: config);

        await client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.GetNextUnreadBundleAsync();

        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Get, "unread");
        auth.RequestContent.ShouldNotBeNull().ShouldContain("\"merchant_credentials\"");
    }

    [Test]
    public async Task PublicConstructor_SendsTheMerchantToken()
    {
        var (client, _, aggregator) = Create(HttpStatusCode.NoContent);

        await new WebhookAggregatorClient(client, MerchantId).GetNextUnreadBundleAsync();

        ShouldCarryTheMerchantToken(aggregator, HttpMethod.Get, "unread");
    }

    [Test]
    public async Task NoToken_NothingIsSentToTheAggregator()
    {
        var (client, _, aggregator) = Create(HttpStatusCode.NoContent, accessToken: null);

        await Should.ThrowAsync<InvalidOperationException>(
            () => client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.GetNextUnreadBundleAsync());

        aggregator.Requests.ShouldBeEmpty();
    }

    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    public async Task RejectedToken_Throws(HttpStatusCode statusCode)
    {
        var (client, _, _) = Create(statusCode);

        await Should.ThrowAsync<Exception>(
            () => client.AsOrOnBehalfOfMerchant(MerchantId).Webhooks.GetNextUnreadBundleAsync());
    }
}
