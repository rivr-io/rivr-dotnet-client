using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Rivr.Models.Authentication;
using Shouldly;
using System.ComponentModel.DataAnnotations;

namespace Rivr.Test;

public class CreateOrderTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task CreateOrderAsync_WhenValidOrder_ReturnsCreatedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var orderResponse = new Order
        {
            Id = orderId,
            MerchantId = _merchantId,
            Reference = "test-ref-123",
            Amount = 125,
            Status = OrderStatus.Created
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(orderResponse, System.Net.HttpStatusCode.Created);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var createOrderRequest = new CreateOrderRequest
        {
            Id = orderId,
            Reference = "test-ref-123",
            PersonalNumber = "199001011234",
            Email = "test@example.com",
            Phone = "0701234567",
            OrderLines =
            [
                new OrderLine
                {
                    Description = "Test Product",
                    Quantity = 1,
                    UnitPriceExclVat = 100,
                    VatPercentage = 25
                }
            ]
        };

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .CreateOrderAsync(createOrderRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(orderId);
        result.Reference.ShouldBe("test-ref-123");
        result.Status.ShouldBe(OrderStatus.Created);

        authHandler.PerformedRequestsCount.ShouldBe(1);
        apiHandler.PerformedRequestsCount.ShouldBe(1);
    }

    [Test]
    public async Task CreateOrderAsync_WhenNoOrderLines_ThrowsValidationException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler();
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var createOrderRequest = new CreateOrderRequest
        {
            Reference = "test-ref-123",
            OrderLines = [] // Empty - should result in Amount = 0
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .CreateOrderAsync(createOrderRequest));

        exception.Message.ShouldContain("Amount");
    }

    [Test]
    public async Task CreateOrderAsync_WhenNoReference_ThrowsValidationException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler();
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var createOrderRequest = new CreateOrderRequest
        {
            Reference = null, // Missing reference
            OrderLines =
            [
                new OrderLine
                {
                    Description = "Test Product",
                    Quantity = 1,
                    UnitPriceExclVat = 100,
                    VatPercentage = 25
                }
            ]
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .CreateOrderAsync(createOrderRequest));

        exception.Message.ShouldContain("Reference");
    }

    [Test]
    public async Task CreateOrderAsync_WhenQuantityIsZero_ThrowsValidationException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler();
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var createOrderRequest = new CreateOrderRequest
        {
            Reference = "test-ref-123",
            OrderLines =
            [
                new OrderLine
                {
                    Description = "Test Product",
                    Quantity = 0, // Invalid quantity
                    UnitPriceExclVat = 100,
                    VatPercentage = 25
                }
            ]
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .CreateOrderAsync(createOrderRequest));

        exception.Message.ShouldContain("Quantity");
    }

    [Test]
    public void CreateOrderRequest_Amount_CalculatesCorrectly()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            OrderLines =
            [
                new OrderLine
                {
                    Description = "Product 1",
                    Quantity = 2,
                    UnitPriceExclVat = 100,
                    VatPercentage = 25
                },
                new OrderLine
                {
                    Description = "Product 2",
                    Quantity = 1,
                    UnitPriceExclVat = 50,
                    VatPercentage = 12
                }
            ]
        };

        // Act
        var amount = request.Amount;

        // Assert
        // Product 1: 2 * 100 * 1.25 = 250
        // Product 2: 1 * 50 * 1.12 = 56
        // Total: 306
        amount.ShouldBe(306);
    }

    [Test]
    public async Task CreateOrderAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var orderResponse = new Order { Id = Guid.NewGuid() };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(orderResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var createOrderRequest = new CreateOrderRequest
        {
            Reference = "test-ref-123",
            OrderLines =
            [
                new OrderLine
                {
                    Description = "Test Product",
                    Quantity = 1,
                    UnitPriceExclVat = 100,
                    VatPercentage = 25
                }
            ]
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .CreateOrderAsync(createOrderRequest, cts.Token));
    }
}
