using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Rivr.Extensions;
using Rivr.Models;
using Rivr.Models.Authentication;
using Rivr.Models.Devices;
using Rivr.Models.Orders;
using Rivr.Models.OrderSettlements;

namespace Rivr;

/// <inheritdoc />
public class MerchantClient(Client client, Guid merchantId) : IMerchantOperations
{
    /// <inheritdoc />
    public async Task<Health> GetHealthSecureAsync()
    {
        await RefreshMerchantCredentialsAsync();

        return await client.GetHealthAsync();
    }

    /// <inheritdoc />
    public async Task<Device[]> GetDevicesAsync()
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"devices");
        await response.EnsureSuccessfulResponseAsync();
        var result = await response.DeserialiseAsync<GetDevicesResponse>();
        return result.Devices;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Order> GetOrderAsync(Guid orderId)
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"orders/{orderId}");
        await response.EnsureSuccessfulResponseAsync();
        return await response.DeserialiseAsync<Order>();
    }

    /// <inheritdoc />
    public async Task RefundAsync(Guid orderId)
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.PostAsync($"orders/{orderId}/refund", null);
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
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"order-settlements");
        await response.EnsureSuccessfulResponseAsync();

        var result = await response.DeserialiseAsync<GetOrderSettlementsResponse>();

        return result.OrderSettlements;
    }


    /// <inheritdoc />

    public async Task<OrderSettlement> GetLastUnreadOrderSettlementAsync()
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"order-settlements/last-unread");
        await response.EnsureSuccessfulResponseAsync();

        return await response.DeserialiseAsync<OrderSettlement>();
    }

    /// <inheritdoc />
    public async Task<string> GetNextUnreadOrderSettlementAsNetsFile()
    {
        await RefreshMerchantCredentialsAsync();

        var response = await client.ApiHttpClient.GetAsync($"order-settlements/next-unread?format=Nets");

        await response.EnsureSuccessfulResponseAsync();

        return await response.Content.ReadAsStringAsync();
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