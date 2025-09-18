using System;
using System.Net;
using System.Threading.Tasks;
using Rivr.Core;
using Rivr.Core.Models.Webhooks;
using Rivr.Extensions;

namespace Rivr;

/// <inheritdoc />
public class WebhookAggregatorClient(Client client, Guid merchantId) : IWebhookAggregatorOperations
{
    private string BaseAddress { get; set; } = $"merchants/{merchantId}/webhook-bundles/";

    /// <inheritdoc />
    public async Task CreateBundleAsync()
    {
        var response = await client.WebhookAggregatorHttpClient.PostAsync(BaseAddress, null);

        await response.EnsureSuccessfulResponseAsync();
    }

    /// <inheritdoc />
    public async Task<Bundle?> GetNextUnreadBundleAsync()
    {
        var response = await client.WebhookAggregatorHttpClient.GetAsync(BaseAddress + "unread");
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
        var response = await client.WebhookAggregatorHttpClient.GetAsync(BaseAddress + bundleId);

        await response.EnsureSuccessfulResponseAsync();

        var result = await response.DeserialiseAsync<GetBundleResponse>();

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        return new Bundle
        {
            BundleId = result.BundleId,
            Webhooks = result.Webhooks
        };
    }

    /// <inheritdoc />
    public async Task MarkBundleAsReadAsync(Guid bundleId)
    {
        var response = await client.WebhookAggregatorHttpClient.PutAsync(BaseAddress + bundleId, null);

        await response.EnsureSuccessfulResponseAsync();
    }
}