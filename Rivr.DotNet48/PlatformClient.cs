using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Merchants;
using Rivr.DotNet48.Models.Authentication;

namespace Rivr.DotNet48;

/// <inheritdoc />
public class PlatformClient : IPlatformOperations
{
    private readonly Client _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformClient"/> class.
    /// </summary>
    /// <param name="client"></param>
    public PlatformClient(Client client)
    {
        this._client = client;
    }

    /// <inheritdoc />
    public async Task<Health> GetHealthSecureAsync()
    {
        await RefreshClientCredentialsAsync();
        return await _client.GetHealthAsync();
    }

    /// <inheritdoc />
    public async Task<GetMerchantsResponse> GetMerchantsAsync()
    {
        await RefreshClientCredentialsAsync();

        var response = await _client.ApiHttpClient.GetAsync("merchants");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GetMerchantsResponse>(content);
    }

    private async Task RefreshClientCredentialsAsync()
    {
        var clientCredentials = new ClientCredentialsTokenRequest(_client.ClientId, _client.ClientSecret);
        var clientCredentialsCacheKey = $"{nameof(Client)}-client-credentials";

        var response = await _client.MemoryCache.GetOrCreateAsync(clientCredentialsCacheKey, async () =>
        {
            var jsonContent = JsonConvert.SerializeObject(clientCredentials);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var authResponse = await _client.AuthHttpClient.PostAsync("connect/token", content);

            if (!authResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {authResponse.StatusCode}");
            }

            var authContent = await authResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TokenResponse>(authContent);
            return result;
        });

        if (response == null)
        {
            throw new InvalidOperationException("Failed to get client credentials token.");
        }

        _client.ApiHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.AccessToken);
    }
}