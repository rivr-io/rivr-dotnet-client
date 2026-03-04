using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
public class PlatformClient : IPlatformOperations
{
    private readonly Client _client;
    private string? _accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformClient"/> class.
    /// </summary>
    /// <param name="client"></param>
    public PlatformClient(Client client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<Health> GetHealthSecureAsync(CancellationToken cancellationToken = default)
    {
        await RefreshClientCredentialsAsync();

        var response = await SendApiGetAsync("health-secure", cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Health>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetMerchantsResponse> GetMerchantsAsync(CancellationToken cancellationToken = default)
    {
        await RefreshClientCredentialsAsync();

        var response = await SendApiGetAsync("merchants", cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<GetMerchantsResponse>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HeartbeatResponse> SendHeartbeatAsync(SendHeartbeatRequest heartbeat, CancellationToken cancellationToken = default)
    {
        await RefreshClientCredentialsAsync();

        var json = JsonSerializer.Serialize(heartbeat);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, $"satellite-services/{heartbeat.UniqueServiceId}/heartbeat") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        var response = await _client.ApiHttpClient.SendAsync(request, cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<HeartbeatResponse>(cancellationToken: cancellationToken);
    }

    private async Task<HttpResponseMessage> SendApiGetAsync(string url, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return await _client.ApiHttpClient.SendAsync(request, cancellationToken);
    }

    private async Task RefreshClientCredentialsAsync()
    {
        var clientCredentials = new ClientCredentialsTokenRequest(_client.Credentials.Id, _client.Credentials.Secret);

        var clientCredentialsCacheKey = $"{nameof(Client)}-client-credentials";
        var response = await _client.MemoryCache.GetOrCreateAsync(clientCredentialsCacheKey, async entry =>
        {
            var response = await _client.AuthHttpClient.PostAsJsonAsync("connect/token", clientCredentials);
            await response.EnsureSuccessfulResponseAsync();
            var result = await response.DeserialiseAsync<TokenResponse>();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.ExpiresIn);
            return result;
        });

        if (response is null)
        {
            throw new InvalidOperationException("Failed to get client credentials token.");
        }

        _accessToken = response.AccessToken;
    }
}