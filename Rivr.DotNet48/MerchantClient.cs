using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rivr.Core;
using Rivr.Core.Models;
using Rivr.Core.Models.Devices;
using Rivr.Core.Models.Orders;
using Rivr.Core.Models.OrderSettlements;
using Rivr.DotNet48.Extensions;
using Rivr.DotNet48.Models.Authentication;

namespace Rivr.DotNet48
{
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
        public MerchantClient(Client client, Guid merchantId)
        {
            this._client = client;
            this._merchantId = merchantId;
        }

        /// <inheritdoc />
        public async Task<Health> GetHealthSecureAsync()
        {
            await RefreshMerchantCredentialsAsync();
            return await _client.GetHealthAsync();
        }

        /// <inheritdoc />
        public async Task<Device[]> GetDevicesAsync()
        {
            await RefreshMerchantCredentialsAsync();

            var response = await _client.ApiHttpClient.GetAsync($"devices");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetDevicesResponse>(content);
            return result.Devices;
        }

        /// <inheritdoc />
        public async Task<Order> CreateOrderAsync(CreateOrderRequest order)
        {
            await RefreshMerchantCredentialsAsync();

            order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
            order.MerchantId = _merchantId;

            var errors = Validate(order);
            if (errors.Length > 0)
            {
                throw new ValidationException(CombineErrors(errors));
            }

            var jsonContent = JsonConvert.SerializeObject(order);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _client.ApiHttpClient.PutAsync($"orders/{order.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Order>(responseContent);
        }

        /// <inheritdoc />
        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            await RefreshMerchantCredentialsAsync();

            var response = await _client.ApiHttpClient.GetAsync($"orders/{orderId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Order>(content);
        }

        /// <inheritdoc />
        public async Task RefundAsync(Guid orderId)
        {
            await RefreshMerchantCredentialsAsync();

            var response = await _client.ApiHttpClient.PostAsync($"orders/{orderId}/refund", null);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorContent);
                throw new ApiCallException(error.Message);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }

        /// <inheritdoc />
        public async Task<OrderSettlementForLists[]> GetOrderSettlementsAsync()
        {
            await RefreshMerchantCredentialsAsync();

            var response = await _client.ApiHttpClient.GetAsync($"order-settlements");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetOrderSettlementsResponse>(content);
            return result.OrderSettlements;
        }

        /// <inheritdoc />
        public async Task<OrderSettlement> GetLastUnreadOrderSettlementAsync()
        {
            await RefreshMerchantCredentialsAsync();

            var response = await _client.ApiHttpClient.GetAsync($"order-settlements/last-unread");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OrderSettlement>(content);
        }

        /// <inheritdoc />
        public async Task<string> GetNextUnreadOrderSettlementAsNetsFile()
        {
            await RefreshMerchantCredentialsAsync();

            var response = await _client.ApiHttpClient.GetAsync($"order-settlements/next-unread?format=Nets");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task RefreshMerchantCredentialsAsync()
        {
            var merchantCredentials = new MerchantCredentialsTokenRequest(_client.ClientId, _client.ClientSecret, _merchantId);
            var merchantCredentialsCacheKey = $"{nameof(Client)}-merchant-credentials-{_merchantId}";

            var response = await _client.MemoryCache.GetOrCreateAsync(merchantCredentialsCacheKey, async () =>
            {
                var jsonContent = JsonConvert.SerializeObject(merchantCredentials);
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
                throw new InvalidOperationException("Failed to get merchant credentials token.");
            }

            _client.ApiHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.AccessToken);
        }

        private static OrderRequestError[] Validate(CreateOrderRequest createOrderRequest)
        {
            var errorMessages = new List<OrderRequestError>();

            if (createOrderRequest == null)
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
                });
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

        private static string CombineErrors(OrderRequestError[] errors)
        {
            var messages = new List<string>();
            foreach (var error in errors)
            {
                messages.Add($"{error.PropertyName}: {error.Message}");
            }

            return string.Join("; ", messages);
        }
    }
}