using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Merchants;
using Rivr.Core.Models.OrderSettlements;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class OrderSettlementTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task GetOrderSettlementsAsync_WhenSuccessful_ReturnsSettlements()
    {
        // Arrange
        var settlementId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var settlementsResponse = new GetOrderSettlementsResponse
        {
            OrderSettlements =
            [
                new OrderSettlementForLists
                {
                    Id = settlementId,
                    MerchantId = _merchantId,
                    MerchantName = "Test Merchant",
                    Reference = "REF-001",
                    TotalAmount = 5000,
                    CompletedCount = 10,
                    RefundedCount = 1,
                    CompletedAmount = 5500,
                    RefundedAmount = 500
                }
            ],
            Metadata = new Metadata { PageNumber = 1, PageSize = 10, TotalItems = 1, TotalPages = 1 }
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(settlementsResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetOrderSettlementsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe(settlementId);
        result[0].Reference.ShouldBe("REF-001");
        result[0].TotalAmount.ShouldBe(5000);
        result[0].CompletedCount.ShouldBe(10);
        result[0].RefundedCount.ShouldBe(1);
    }

    [Test]
    public async Task GetOrderSettlementsAsync_WhenEmpty_ReturnsEmptyArray()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var settlementsResponse = new GetOrderSettlementsResponse
        {
            OrderSettlements = [],
            Metadata = new Metadata { PageNumber = 1, PageSize = 10, TotalItems = 0, TotalPages = 0 }
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(settlementsResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetOrderSettlementsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(0);
    }

    [Test]
    public async Task GetNextUnreadOrderSettlementAsync_WhenSuccessful_ReturnsSettlement()
    {
        // Arrange
        var settlementId = Guid.NewGuid();
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var settlementResponse = new OrderSettlement
        {
            Id = settlementId,
            MerchantId = _merchantId,
            MerchantName = "Test Merchant",
            Reference = "REF-002",
            SettlementNumber = "S-2024-001"
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(settlementResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetNextUnreadOrderSettlementAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(settlementId);
        result.Reference.ShouldBe("REF-002");
        result.SettlementNumber.ShouldBe("S-2024-001");
    }

    [Test]
    public async Task GetNextUnreadOrderSettlementAsNetsFile_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.NotFound);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetNextUnreadOrderSettlementAsNetsFile();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetOrderSettlementsAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var settlementsResponse = new GetOrderSettlementsResponse { OrderSettlements = [] };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(settlementsResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetOrderSettlementsAsync(cts.Token));
    }
}
