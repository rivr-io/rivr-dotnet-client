using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
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
        _config = new Config(clientId: TestConstants.ClientId, clientSecret: TestConstants.ClientSecret)
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
            .AsPlatform()
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
            .AsPlatform()
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

        var merchantId = TestConstants.MerchantId;

        // Act
        var result = await client
            .OnBehalfOfMerchant(merchantId)
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

        var merchantId = TestConstants.MerchantId;

        // Act
        var result = await client
            .OnBehalfOfMerchant(merchantId)
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

        var merchantId = TestConstants.MerchantId;

        var order = new CreateOrderRequest
        {
            Id = Guid.NewGuid(),
            PersonalNumber = TestConstants.PersonalNumber,
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
            .OnBehalfOfMerchant(merchantId)
            .CreateOrderAsync(order);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Reference.ShouldBe(order.Reference);
        result.Amount.ShouldBe(order.Amount);

        Console.WriteLine($"Order ID: {result.Id}");
    }

    [Test]
    public async Task ShouldGetOrder()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var merchantId = TestConstants.MerchantId;

        var orderId = Guid.Parse("65738986-e888-47f9-b7b7-050de7823b8d");

        // Act
        var result = await client
            .OnBehalfOfMerchant(merchantId)
            .GetOrderAsync(orderId);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldRefundOrder()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var merchantId = TestConstants.MerchantId;

        var orderId = Guid.Parse("65738986-e888-47f9-b7b7-050de7823b8d");

        // Act
        await client
            .OnBehalfOfMerchant(merchantId)
            .RefundAsync(orderId);
    }
}