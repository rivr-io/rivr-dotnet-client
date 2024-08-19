﻿using Rivr.Extensions;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Environment = Rivr.Models.Environment;
using Rivr.Models;

namespace Rivr;

/// <inheritdoc />
public class Client : IClient
{
    /// <inheritdoc />
    public Config Config { get; }

    internal readonly HttpClient AuthHttpClient;
    internal readonly HttpClient ApiHttpClient;
    internal readonly IMemoryCache MemoryCache;
    internal readonly string ClientId;
    internal readonly string ClientSecret;
    private PlatformApi? _platformOperations;
    private readonly ConcurrentDictionary<Guid, MerchantClient> _merchantClients = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="memoryCache"></param>
    public Client(Config config, IMemoryCache memoryCache) : this(new HttpClient(), new HttpClient(), config, memoryCache)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="authHttpClient"></param>
    /// <param name="apiHttpClient"></param>
    /// <param name="config"></param>
    /// <param name="memoryCache"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Client(HttpClient authHttpClient, HttpClient apiHttpClient, Config config, IMemoryCache memoryCache)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        ClientId = config.ClientId ?? throw new ArgumentNullException(nameof(config.ClientId));
        ClientSecret = config.ClientSecret ?? throw new ArgumentNullException(nameof(config.ClientSecret));

        AuthHttpClient = authHttpClient;
        ApiHttpClient = apiHttpClient;
        MemoryCache = memoryCache;

        AuthHttpClient.BaseAddress = Config.Environment == Environment.Production
            ? new Uri(Config.AuthBaseUri)
            : new Uri(Config.AuthBaseUriTest);

        ApiHttpClient.BaseAddress = Config.Environment == Environment.Production
            ? new Uri(Config.ApiBaseUri)
            : new Uri(Config.ApiBaseUriTest);
    }


    /// <inheritdoc />
    public async Task<Health> GetHealthAsync()
    {
        var response = await ApiHttpClient.GetAsync("health");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Health>();
    }

    /// <inheritdoc />
    public IPlatformOperations AsPlatform() => _platformOperations ??= new PlatformApi(this);

    /// <inheritdoc />
    public IMerchantOperations OnBehalfOfMerchant(Guid merchantId) => _merchantClients[merchantId] ??= new MerchantClient(this, merchantId);
}