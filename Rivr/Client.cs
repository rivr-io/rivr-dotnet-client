using Rivr.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Models.Authentication;
using Rivr.Models.Devices;
using Environment = Rivr.Models.Environment;
using Rivr.Models.Merchants;
using Rivr.Models;
using Rivr.Models.Orders;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Rivr.Extensions;

namespace Rivr;

public interface IClient
{
    Config Config { get; }
    Task<Health> GetHealthAsync();
    IPlatformOperations AsPlatform { get; }
    IMerchantOperations OnBehalfOfMerchant(Guid merchantId);
}

public interface IPlatformOperations
{
    Task<Health> GetHealthSecureAsync();
    Task<GetMerchantsResponse> GetMerchantsAsync();
}

public interface IMerchantOperations
{
    Task<Health> GetHealthSecureAsync();
    Task<MerchantDevice[]> GetDevicesAsync();
    Task<Order> CreateOrderAsync(CreateOrderRequest order);
    Task<Order> GetOrderAsync(Guid orderId);
}

public class Client : IClient
{
    public Config Config { get; }
    internal readonly HttpClient AuthHttpClient;
    internal readonly HttpClient ApiHttpClient;
    internal readonly IMemoryCache MemoryCache;
    internal readonly string ClientId;
    internal readonly string ClientSecret;

    public Client(Config config, IMemoryCache memoryCache) : this(new HttpClient(), new HttpClient(), config, memoryCache)
    {
    }

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


    public async Task<Health> GetHealthAsync()
    {
        var response = await ApiHttpClient.GetAsync("health");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Health>();
    }

    private async Task<Health> GetHealthSecureAsync()
    {
        var response = await ApiHttpClient.GetAsync("health-secure");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Health>();
    }

    public IPlatformOperations AsPlatform => new PlatformApi(this);

    public IMerchantOperations OnBehalfOfMerchant(Guid merchantId)
    {
        return new MerchantClient(this, merchantId);
    }
}

public class MerchantClient(Client client, Guid merchantId) : IMerchantOperations
{
    public async Task<Health> GetHealthSecureAsync()
    {
        await RefreshMerchantCredentialsAsync();

        return await client.GetHealthAsync();
    }

    public async Task<MerchantDevice[]> GetDevicesAsync()
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"devices");
        await response.EnsureSuccessfulResponseAsync();
        var result = await response.DeserialiseAsync<GetDevicesResponse>();
        return result.Devices;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest order)
    {
        await RefreshMerchantCredentialsAsync();

        order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
        order.MerchantId = merchantId;

        var errors = Validate(order);
        if (errors.Length > 0)
        {
            throw new ValidationException(errors.CombineToString());
        }

        var response = await client.ApiHttpClient.PutAsJsonAsync($"orders/{order.Id}", order);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Order>();
    }

    public async Task<Order> GetOrderAsync(Guid orderId)
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"orders/{orderId}");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Order>();
    }


    private async Task RefreshMerchantCredentialsAsync()
    {
        var merchantCredentials = new MerchantCredentialsTokenRequest(client.ClientId, client.ClientSecret, merchantId);

        var merchantCredentialsCacheKey = $"{nameof(Client)}-merchant-credentials-{merchantId}";
        var response = await client.MemoryCache.GetOrCreateAsync(merchantCredentialsCacheKey, async entry =>
        {
            var response = await client.AuthHttpClient.PostAsJsonAsync("connect/token", merchantCredentials);
            await response.EnsureSuccessfulResponseAsync();
            var result = await response.DeserialiseAsync<TokenResponse>();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.ExpiresIn);
            return result;
        });

        if (response is null)
        {
            throw new InvalidOperationException("Failed to get merchant credentials token.");
        }

        client.ApiHttpClient.DefaultRequestHeaders.Authorization = new("Bearer", response.AccessToken);
    }

    static PaymentRequestError[] Validate(CreateOrderRequest? createOrderRequest)
    {
        var errorMessages = new List<PaymentRequestError>();

        if (createOrderRequest is null)
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "Request is required",
                PropertyName = nameof(createOrderRequest)
            });
            return errorMessages.ToArray();
        }

        if (createOrderRequest.Id == Guid.Empty)
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "Id must be a valid UUID",
                PropertyName = nameof(createOrderRequest.Id)
            });
        }

        if (string.IsNullOrEmpty(createOrderRequest.PersonalNumber))
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "PersonalNumber is required",
                PropertyName = nameof(createOrderRequest.PersonalNumber)
            });
        }

        if (createOrderRequest.Amount <= 0)
        {
            errorMessages.Add(new PaymentRequestError
                {
                    Message = "Amount must be greater than 0",
                    PropertyName = nameof(createOrderRequest.Amount)
                }
            );
        }

        if (string.IsNullOrEmpty(createOrderRequest.Email))
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "Email is required",
                PropertyName = nameof(createOrderRequest.Email)
            });
        }

        if (createOrderRequest.MerchantId == Guid.Empty)
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "MerchantId is required",
                PropertyName = nameof(createOrderRequest.MerchantId)
            });
        }

        if (string.IsNullOrEmpty(createOrderRequest.Phone))
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "Phone is required",
                PropertyName = nameof(createOrderRequest.Phone)
            });
        }

        if (string.IsNullOrEmpty(createOrderRequest.Reference))
        {
            errorMessages.Add(new PaymentRequestError
            {
                Message = "Reference is required",
                PropertyName = nameof(createOrderRequest.Reference)
            });
        }

        foreach (var orderLine in createOrderRequest.OrderLines)
        {
            if (orderLine.Quantity <= 0)
            {
                errorMessages.Add(new PaymentRequestError
                {
                    Message = "Quantity must be greater than 0",
                    PropertyName = nameof(orderLine.Quantity)
                });
            }

            if (orderLine.VatPercentage < 0)
            {
                errorMessages.Add(new PaymentRequestError
                {
                    Message = "VatPercentage must be greater than or equal to 0",
                    PropertyName = nameof(orderLine.VatPercentage)
                });
            }
        }

        return errorMessages.ToArray();
    }
}

public class PlatformApi(Client client) : IPlatformOperations
{
    public async Task<Health> GetHealthSecureAsync()
    {
        await RefreshClientCredentialsAsync();

        return await client.GetHealthAsync();
    }

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