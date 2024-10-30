using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Merchants;
using Rivr.Extensions;
using Rivr.Models.Authentication;

namespace Rivr;

/// <inheritdoc />
public class PlatformClient(Client client) : IPlatformOperations
{
    /// <inheritdoc />
    public async Task<Health> GetHealthSecureAsync()
    {
        await RefreshClientCredentialsAsync();

        return await client.GetHealthAsync();
    }

    /// <inheritdoc />
    public async Task<GetMerchantsResponse> GetMerchantsAsync()
    {
        await RefreshClientCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync("merchants");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<GetMerchantsResponse>();
    }

    private async Task RefreshClientCredentialsAsync()
    {
        var clientCredentials = new ClientCredentialsTokenRequest(client.ClientId, client.ClientSecret);

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