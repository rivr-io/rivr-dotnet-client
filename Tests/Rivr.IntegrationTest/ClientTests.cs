using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Rivr.Core.Models;
using Rivr.Core.Models.Orders;
using Shouldly;
using Environment = Rivr.Core.Models.Environment;

namespace Rivr.IntegrationTest;

public class ClientTests
{
    private Config _config;

    [SetUp]
    public void Setup()
    {
        _config = new Config(
            clientId: TestConstants.ClientId!,
            clientSecret: TestConstants.ClientSecret!
        )
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
        var options = Options.Create(new MemoryCacheOptions());
        var memoryCache = new MemoryCache(options);
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
            .AsOrOnBehalfOfMerchant(merchantId)
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
            .AsOrOnBehalfOfMerchant(merchantId)
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
            PersonalNumber = TestConstants.PersonalNumber!,
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
            .AsOrOnBehalfOfMerchant(merchantId)
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

        var orderId = Guid.Parse("fd4cc3c6-b7ad-48a9-bc66-b1b224449de5");

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(merchantId)
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

        var orderId = Guid.Parse("fd4cc3c6-b7ad-48a9-bc66-b1b224449de5");

        // Act
        await client
            .AsOrOnBehalfOfMerchant(merchantId)
            .RefundAsync(orderId);
    }

    [Test]
    public async Task ShouldGetNextUnreadOrderSettlementAsNetsFile()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(_config, memoryCache);

        var merchantId = TestConstants.MerchantId;

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(merchantId)
            .GetNextUnreadOrderSettlementAsNetsFile();

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldNotThrowObjectDisposedExceptionWhenHavingWrongCredentials()
    {
        // Arrange
        var memoryCache = Substitute.For<IMemoryCache>();
        var client = new Client(new Config(clientId: Guid.NewGuid().ToString(), clientSecret: "wrong"), memoryCache);

        // Act
        var health = await client
            .GetHealthAsync();

        var healthSecure = await client
            .AsPlatform()
            .GetHealthSecureAsync();

        // Assert
    }
}