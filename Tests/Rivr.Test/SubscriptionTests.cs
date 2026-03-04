using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Core.Models.Subscriptions;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class SubscriptionTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task CreateOrUpdateSubscriptionAsync_WhenSuccessful_CompletesWithoutException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.OK);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var subscriptionRequest = new CreateSubscriptionRequest
        {
            MerchantId = _merchantId,
            PersonalNumber = "199001011234",
            Email = "test@example.com",
            Phone = "0701234567",
            SubscriptionPayments =
            [
                new SubscriptionPayment
                {
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    Amount = 299,
                    Description = "Monthly subscription",
                    ExternalReference = "SUB-001"
                }
            ]
        };

        // Act & Assert - Should not throw
        await client
            .AsOrOnBehalfOfMerchant(_merchantId)
            .CreateOrUpdateSubscriptionAsync(subscriptionRequest);

        authHandler.PerformedRequestsCount.ShouldBe(1);
        apiHandler.PerformedRequestsCount.ShouldBe(1);
    }

    [Test]
    public async Task CreateOrUpdateSubscriptionAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.OK);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        var subscriptionRequest = new CreateSubscriptionRequest
        {
            MerchantId = _merchantId,
            PersonalNumber = "199001011234"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .CreateOrUpdateSubscriptionAsync(subscriptionRequest, cts.Token));
    }
}
