using Rivr.Extensions;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Environment = Rivr.Core.Models.Environment;

namespace Rivr;

/// <inheritdoc />
public class Client : IClient
{
    /// <inheritdoc />
    public Config Config { get; }


    internal readonly HttpClient AuthHttpClient;
    internal readonly HttpClient ApiHttpClient;
    internal readonly HttpClient WebhookAggregatorHttpClient;
    internal readonly IMemoryCache MemoryCache;

    internal readonly Credentials Credentials;

    private PlatformClient? _platformOperations;
    private readonly ConcurrentDictionary<Guid, MerchantClient> _merchantClients = new();

    internal JsonSerializerOptions JsonSerializerOptions => new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="memoryCache"></param>
    public Client(Config config, IMemoryCache memoryCache) : this(new HttpClient(), new HttpClient(), new HttpClient(), config, memoryCache)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="authHttpClient"></param>
    /// <param name="apiHttpClient"></param>
    /// <param name="webhookAggregatorHttpClient"></param>
    /// <param name="config"></param>
    /// <param name="memoryCache"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Client(HttpClient authHttpClient, HttpClient apiHttpClient, HttpClient webhookAggregatorHttpClient, Config config, IMemoryCache memoryCache)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        Credentials = new Credentials(config);

        AuthHttpClient = authHttpClient;
        ApiHttpClient = apiHttpClient;
        WebhookAggregatorHttpClient = webhookAggregatorHttpClient;
        MemoryCache = memoryCache;

        AuthHttpClient.BaseAddress = Config.Environment == Environment.Production
            ? new Uri(Config.AuthBaseUri)
            : new Uri(Config.AuthBaseUriTest);

        ApiHttpClient.BaseAddress = Config.Environment == Environment.Production
            ? new Uri(Config.ApiBaseUri)
            : new Uri(Config.ApiBaseUriTest);

        WebhookAggregatorHttpClient.BaseAddress = Config.Environment == Environment.Production
            ? new Uri(Config.WebhookAggregatorBaseUri)
            : new Uri(Config.WebhookAggregatorBaseUriTest);
    }

    /// <inheritdoc />
    public bool IsConfiguredForSingleMerchant => Credentials.GrantType == GrantTypes.MerchantCredentials;

    /// <inheritdoc />
    public Guid ConfiguredMerchantId => IsConfiguredForSingleMerchant ? Guid.Parse(Credentials.Id) : throw new InvalidOperationException("Client is not configured with Merchant Credentials.");

    /// <inheritdoc />
    public async Task<Health> GetHealthAsync()
    {
        var response = await ApiHttpClient.GetAsync("health");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Health>();
    }

    /// <inheritdoc />
    public IPlatformOperations AsPlatform() => _platformOperations ??= new PlatformClient(this);

    /// <inheritdoc />
    public IMerchantOperations AsOrOnBehalfOfMerchant(Guid merchantId) => _merchantClients.ContainsKey(merchantId)
        ? _merchantClients[merchantId]
        : _merchantClients[merchantId] = new MerchantClient(this, merchantId);
}

internal class Credentials
{
    public string GrantType { get; set; }
    public string Id { get; set; }
    public string Secret { get; set; }

    public Credentials(Config config)
    {
        var hasClientId = !string.IsNullOrEmpty(config.ClientId);
        var hasClientSecret = !string.IsNullOrEmpty(config.ClientSecret);
        var hasMerchantId = !string.IsNullOrEmpty(config.MerchantId);
        var hasMerchantSecret = !string.IsNullOrEmpty(config.MerchantSecret);

        if (!hasClientId && !hasMerchantId)
        {
            throw new ArgumentException("Either ClientId or MerchantId must be provided in the configuration.");
        }

        if (hasClientId && !hasClientSecret)
        {
            throw new ArgumentException("ClientSecret must be provided in the configuration when ClientId is provided.");
        }

        if (hasMerchantId && !hasMerchantSecret)
        {
            throw new ArgumentException("MerchantSecret must be provided in the configuration when MerchantId is provided.");
        }

        if (hasClientId && hasMerchantId)
        {
            throw new ArgumentException("Only one of ClientId or MerchantId should be provided in the configuration, not both.");
        }

        if (hasMerchantId)
        {
            GrantType = GrantTypes.MerchantCredentials;

            Id = config.MerchantId;
            Secret = config.MerchantSecret;

            return;
        }

        if (hasClientId)
        {
            GrantType = GrantTypes.ClientCredentials;

            Id = config.ClientId;
            Secret = config.ClientSecret;

            return;
        }

        throw new ArgumentException("Invalid configuration for credentials.");
    }
}

/// <summary>
/// Constants for OAuth2 grant types.
/// </summary>
public static class GrantTypes
{
    /// <summary>
    /// Client Credentials Grant
    /// </summary>
    public const string ClientCredentials = "client_credentials";

    /// <summary>
    /// Merchant Credentials Grant
    /// </summary>
    public const string MerchantCredentials = "merchant_credentials";

    /// <summary>
    /// Merchant Token Grant
    /// </summary>
    public const string MerchantToken = "merchant_token";
}