using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Heartbeats;
using Rivr.Core.Models.SatelliteServices;
using Rivr.Core.Models.Merchants;
using Rivr.Extensions;
using Rivr.Models.Authentication;

namespace Rivr;

/// <inheritdoc />
public class PlatformClient(Client client) : IPlatformOperations
{
    /// <inheritdoc />
    public async Task<Health> GetHealthSecureAsync(CancellationToken cancellationToken = default)
    {
        await RefreshClientCredentialsAsync();

        return await client.GetHealthAsync();
    }

    /// <inheritdoc />
    public async Task<GetMerchantsResponse> GetMerchantsAsync(CancellationToken cancellationToken = default)
    {
        await RefreshClientCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync("merchants", cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<GetMerchantsResponse>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HeartbeatResponse> SendHeartbeat(SendHeartbeatRequest heartbeat, CancellationToken cancellationToken = default)
    {
        await RefreshClientCredentialsAsync();

        var response = await client.ApiHttpClient.PutAsJsonAsync($"satellite-services/{heartbeat.UniqueServiceId}/heartbeat", heartbeat, cancellationToken: cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<HeartbeatResponse>(cancellationToken: cancellationToken);
    }

    private async Task RefreshClientCredentialsAsync()
    {
        var clientCredentials = new ClientCredentialsTokenRequest(client.Credentials.Id, client.Credentials.Secret);

        var clientCredentialsCacheKey = $"{nameof(Client)}-client-credentials";
        var response = await client.MemoryCache.GetOrCreateAsync(clientCredentialsCacheKey, async entry =>
        {
            var response = await client.AuthHttpClient.PostAsJsonAsync("connect/token", clientCredentials);
            await response.EnsureSuccessfulResponseAsync();
            var result = await response.DeserialiseAsync<TokenResponse>();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.ExpiresIn);
            return result;
        });

        if (response is null)
        {
            throw new InvalidOperationException("Failed to get client credentials token.");
        }

        client.ApiHttpClient.DefaultRequestHeaders.Authorization = new("Bearer", response.AccessToken);
    }
}