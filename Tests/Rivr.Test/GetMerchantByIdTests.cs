using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Merchants;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class GetMerchantByIdTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task GetMerchantAsync_WhenSuccessful_ReturnsMerchant()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var merchantResponse = new GetMerchantByIdResponse
        {
            Merchant = new Merchant
            {
                Id = _merchantId,
                Name = "Test Merchant AB",
                OrganisationNumber = "5566778899",
                ExternalId = "ext-001"
            }
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(merchantResponse);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .GetMerchantAsync(_merchantId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_merchantId);
        result.Name.ShouldBe("Test Merchant AB");
        result.OrganisationNumber.ShouldBe("5566778899");
        result.ExternalId.ShouldBe("ext-001");
    }

    [Test]
    public async Task GetMerchantAsync_WhenEmptyMerchantId_ThrowsArgumentException()
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

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetMerchantAsync(Guid.Empty));

        exception.Message.ShouldContain("MerchantId is required");
    }

    [Test]
    public async Task GetMerchantAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var merchantResponse = new GetMerchantByIdResponse { Merchant = new Merchant() };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(merchantResponse);
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
                .GetMerchantAsync(_merchantId, cts.Token));
    }
}
