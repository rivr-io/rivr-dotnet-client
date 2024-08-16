using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Models;
using Rivr.Models.Orders;
using Shouldly;
using Environment = Rivr.Models.Environment;

namespace Rivr.IntegrationTest;

public class ClientTests
{
    private Config _config;

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "insert-client-id-here", clientSecret: "insert-client-secret-here")
        {
            Environment = Environment.Test
        };
    }

    [Test]
    public async Task ShouldGetHealth()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        // Act
        var result = await client.GetHealthAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("OK");
    }

    [Test]
    public async Task ShouldGetHealthSecureAsPlatform()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        // Act
        var result = await client
            .AsPlatform
            .GetHealthSecureAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("OK");
    }

    [Test]
    public async Task ShouldGetMerchants()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        // Act
        var result = await client
            .AsPlatform
            .GetMerchantsAsync();

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldGetHealthSecureAsMerchant()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var sdkMerchantId = Guid.Parse("insert-merchant-id-here");

        // Act
        var result = await client
            .OnBehalfOfMerchant(sdkMerchantId)
            .GetHealthSecureAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("OK");
    }

    [Test]
    public async Task ShouldGetMerchantDevices()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var sdkMerchantId = Guid.Parse("insert-merchant-id-here");

        // Act
        var result = await client
            .OnBehalfOfMerchant(sdkMerchantId)
            .GetDevicesAsync();

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldCreateOrder()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var sdkMerchantId = Guid.Parse("insert-merchant-id-here");

        var order = new CreateOrderRequest
        {
            Id = Guid.NewGuid(),
            PersonalNumber = "190001010101",
            Email = "test@example.com",
            Phone = "0700000000",
            CheckoutHints =
            [
                new CheckoutHint
                {
                    Type = CheckoutHintType.Device,
                    Value = "insert-device-id"
                }
            ],
            Reference = "123456",
            Metadata = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            CallbackUrl = "https://example.com/callback",
            OrderLines =
            [
                new OrderLine
                {
                    Description = "Test Product",
                    Quantity = 1,
                    UnitPriceExclVat = 100,
                    VatPercentage = 0
                }
            ]
        };

        // Act
        var result = await client
            .OnBehalfOfMerchant(sdkMerchantId)
            .CreateOrderAsync(order);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Reference.ShouldBe(order.Reference);
        result.Amount.ShouldBe(order.Amount);
    }

    [Test]
    public async Task ShouldGetOrder()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var sdkMerchantId = Guid.Parse("insert-merchant-id-here");

        var orderId = Guid.Parse("insert-order-id-here");

        // Act
        var result = await client
            .OnBehalfOfMerchant(sdkMerchantId)
            .GetOrderAsync(orderId);

        // Assert
        result.ShouldNotBeNull();
    }
}