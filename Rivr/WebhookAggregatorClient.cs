using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Rivr.Core;
using Rivr.Core.Models.Webhooks;
using Rivr.Extensions;

namespace Rivr;

/// <inheritdoc />
/// <remarks>
/// Every call carries the merchant's access token, the same token as the calls to the API. The webhook aggregator
/// answers 401 without a token and 403 for a token that belongs to another merchant.
/// </remarks>
public class WebhookAggregatorClient : IWebhookAggregatorOperations
{
    private readonly Client _client;
    private readonly Func<Task<string?>> _getAccessToken;
    private readonly string _baseAddress;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookAggregatorClient"/> class. The access token is fetched with
    /// the credentials of <paramref name="client"/>, for <paramref name="merchantId"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="merchantId"></param>
    public WebhookAggregatorClient(Client client, Guid merchantId)
        : this(client, merchantId, new MerchantClient(client, merchantId).GetAccessTokenAsync)
    {
    }

    internal WebhookAggregatorClient(Client client, Guid merchantId, Func<Task<string?>> getAccessToken)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _getAccessToken = getAccessToken ?? throw new ArgumentNullException(nameof(getAccessToken));
        _baseAddress = $"merchants/{merchantId}/webhook-bundles/";
    }

    /// <inheritdoc />
    public async Task CreateBundleAsync()
    {
        var response = await SendAsync(HttpMethod.Post, _baseAddress);

        await response.EnsureSuccessfulResponseAsync();
    }

    /// <inheritdoc />
    public async Task<Bundle?> GetNextUnreadBundleAsync()
    {
        var response = await SendAsync(HttpMethod.Get, _baseAddress + "unread");
        await response.EnsureSuccessfulResponseAsync();

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        var result = await response.DeserialiseAsync<GetBundleResponse>();

        return new Bundle
        {
            BundleId = result.BundleId,
            Webhooks = result.Webhooks
        };
    }

    /// <inheritdoc />
    public async Task<Bundle?> GetBundleAsync(Guid bundleId)
    {
        var response = await SendAsync(HttpMethod.Get, _baseAddress + bundleId);

        await response.EnsureSuccessfulResponseAsync();

        // 204 has no body, so it is checked before the body is read.
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        var result = await response.DeserialiseAsync<GetBundleResponse>();

        return new Bundle
        {
            BundleId = result.BundleId,
            Webhooks = result.Webhooks
        };
    }

    /// <inheritdoc />
    public async Task MarkBundleAsReadAsync(Guid bundleId)
    {
        var response = await SendAsync(HttpMethod.Put, _baseAddress + bundleId);

        await response.EnsureSuccessfulResponseAsync();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url)
    {
        var accessToken = await _getAccessToken();
        if (string.IsNullOrEmpty(accessToken))
        {
            // Never send the request without a token: the aggregator would answer 401, and the reason would be hidden.
            throw new InvalidOperationException("Failed to get merchant token.");
        }

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.WebhookAggregatorHttpClient.SendAsync(request);
    }
}
