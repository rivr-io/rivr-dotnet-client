using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Devices;
using Rivr.Core.Models.Heartbeats;
using Rivr.Core.Models.Merchants;
using Rivr.Core.Models.Orders;
using Rivr.Core.Models.OrderSettlements;
using Rivr.Core.Models.Subscriptions;
using Rivr.Extensions;
using Rivr.Models.Authentication;

namespace Rivr;

/// <inheritdoc />
public class MerchantClient : IMerchantOperations
{
    private readonly Client _client;
    private readonly Guid _merchantId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MerchantClient"/> class.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="merchantId"></param>
    public MerchantClient(Client client, Guid? merchantId = null)
    {
        _client = client;
        _merchantId = client.IsConfiguredForSingleMerchant && Guid.TryParse(client.Credentials.Id, out var merchantIdFromId)
            ? merchantIdFromId
            : merchantId ?? throw new InvalidOperationException("MerchantId is required when client is not configured with Merchant Credentials.");
    }


    /// <inheritdoc />
    public async Task<Health> GetHealthSecureAsync()
    {
        await RefreshAccessTokenAsync();

        return await _client.GetHealthAsync();
    }

    /// <inheritdoc />
    public async Task<Device[]> GetDevicesAsync()
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.GetAsync($"devices");
        await response.EnsureSuccessfulResponseAsync();
        var result = await response.DeserialiseAsync<GetDevicesResponse>();
        return result.Devices;
    }

    /// <inheritdoc />
    public async Task<Order> CreateOrderAsync(CreateOrderRequest order)
    {
        await RefreshAccessTokenAsync();

        order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
        order.MerchantId = _merchantId;

        var errors = Validate(order);
        if (errors.Length > 0)
        {
            throw new ValidationException(errors.CombineToString());
        }

        var response = await _client.ApiHttpClient.PutAsJsonAsync($"orders/{order.Id}", order);
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Order>();
    }

    /// <inheritdoc />
    public async Task<Order> GetOrderAsync(Guid orderId)
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.GetAsync($"orders/{orderId}");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Order>(_client.JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task RefundAsync(Guid orderId)
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.PostAsync($"orders/{orderId}/refund", null);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.DeserialiseAsync<ApiErrorResponse>();
            throw new ApiCallException(error.Message);
        }

        await response.EnsureSuccessfulResponseAsync();
    }

    /// <inheritdoc />
    public async Task<OrderSettlementForLists[]> GetOrderSettlementsAsync()
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.GetAsync($"order-settlements");
        await response.EnsureSuccessfulResponseAsync();

        var result = await response.DeserialiseAsync<GetOrderSettlementsResponse>();

        return result.OrderSettlements;
    }


    /// <inheritdoc />
    public async Task<OrderSettlement> GetLastUnreadOrderSettlementAsync()
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.GetAsync($"order-settlements/last-unread");
        await response.EnsureSuccessfulResponseAsync();

        return await response.DeserialiseAsync<OrderSettlement>();
    }

    /// <inheritdoc />
    public async Task<string?> GetNextUnreadOrderSettlementAsNetsFile()
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.GetAsync($"order-settlements/next-unread?format=Nets");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessfulResponseAsync();

        return await response.Content.ReadAsStringAsync();
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateSubscriptionAsync(CreateSubscriptionRequest createSubscriptionRequest)
    {
        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.PostAsJsonAsync($"subscriptions", createSubscriptionRequest);

        await response.EnsureSuccessfulResponseAsync();
    }

    /// <inheritdoc />
    public async Task SendHeartbeatAsync(SendHeartbeatRequest heartbeat, CancellationToken cancellationToken = default)
    {
        if (heartbeat is null)
        {
            throw new ArgumentNullException(nameof(heartbeat));
        }

        if (string.IsNullOrEmpty(heartbeat.UniqueServiceId))
        {
            throw new ArgumentException("UniqueServiceId is required", nameof(heartbeat.UniqueServiceId));
        }

        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.PutAsJsonAsync($"satellite-services/{heartbeat.UniqueServiceId}/heartbeat", heartbeat, cancellationToken: cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
    }

    /// <inheritdoc />
    public IWebhookAggregatorOperations Webhooks => new WebhookAggregatorClient(_client, _merchantId);

    /// <inheritdoc />
    public async Task<Merchant> GetMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        if (merchantId == Guid.Empty)
        {
            throw new ArgumentException("MerchantId is required", nameof(merchantId));
        }

        await RefreshAccessTokenAsync();

        var response = await _client.ApiHttpClient.GetAsync($"merchants/{merchantId}", cancellationToken);
        await response.EnsureSuccessfulResponseAsync();
        var result = await response.DeserialiseAsync<GetMerchantByIdResponse>(cancellationToken: cancellationToken);
        return result.Merchant;
    }

    private async Task RefreshAccessTokenAsync()
    {
        object merchantCredentials = _client.IsConfiguredForSingleMerchant
            ? new MerchantCredentialsRequest(_client.Credentials.Id, _client.Credentials.Secret)
            : new MerchantTokenRequest(_client.Credentials.Id, _client.Credentials.Secret, _merchantId);

        var asd = JsonSerializer.Serialize(merchantCredentials, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
        });

        var merchantCredentialsCacheKey = $"{nameof(Client)}-merchant-token-{_merchantId}";
        var response = await _client.MemoryCache.GetOrCreateAsync(merchantCredentialsCacheKey, async entry =>
        {
            var response = await _client.AuthHttpClient.PostAsJsonAsync("connect/token", merchantCredentials);
            await response.EnsureSuccessfulResponseAsync();
            var result = await response.DeserialiseAsync<TokenResponse>();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.ExpiresIn);
            return result;
        });

        if (response is null)
        {
            throw new InvalidOperationException("Failed to get merchant token.");
        }

        _client.ApiHttpClient.DefaultRequestHeaders.Authorization = new("Bearer", response.AccessToken);
    }

    static OrderRequestError[] Validate(CreateOrderRequest? createOrderRequest)
    {
        var errorMessages = new List<OrderRequestError>();

        if (createOrderRequest is null)
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "Request is required",
                PropertyName = nameof(createOrderRequest)
            });
            return errorMessages.ToArray();
        }

        if (createOrderRequest.Id == Guid.Empty)
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "Id must be a valid UUID",
                PropertyName = nameof(createOrderRequest.Id)
            });
        }

        if (string.IsNullOrEmpty(createOrderRequest.PersonalNumber))
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "PersonalNumber is required",
                PropertyName = nameof(createOrderRequest.PersonalNumber)
            });
        }

        if (createOrderRequest.Amount <= 0)
        {
            errorMessages.Add(new OrderRequestError
                {
                    Message = "Amount must be greater than 0",
                    PropertyName = nameof(createOrderRequest.Amount)
                }
            );
        }

        if (string.IsNullOrEmpty(createOrderRequest.Email))
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "Email is required",
                PropertyName = nameof(createOrderRequest.Email)
            });
        }

        if (createOrderRequest.MerchantId == Guid.Empty)
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "MerchantId is required",
                PropertyName = nameof(createOrderRequest.MerchantId)
            });
        }

        if (string.IsNullOrEmpty(createOrderRequest.Phone))
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "Phone is required",
                PropertyName = nameof(createOrderRequest.Phone)
            });
        }

        if (string.IsNullOrEmpty(createOrderRequest.Reference))
        {
            errorMessages.Add(new OrderRequestError
            {
                Message = "Reference is required",
                PropertyName = nameof(createOrderRequest.Reference)
            });
        }

        foreach (var orderLine in createOrderRequest.OrderLines)
        {
            if (orderLine.Quantity <= 0)
            {
                errorMessages.Add(new OrderRequestError
                {
                    Message = "Quantity must be greater than 0",
                    PropertyName = nameof(orderLine.Quantity)
                });
            }

            if (orderLine.VatPercentage < 0)
            {
                errorMessages.Add(new OrderRequestError
                {
                    Message = "VatPercentage must be greater than or equal to 0",
                    PropertyName = nameof(orderLine.VatPercentage)
                });
            }
        }

        return errorMessages.ToArray();
    }
}