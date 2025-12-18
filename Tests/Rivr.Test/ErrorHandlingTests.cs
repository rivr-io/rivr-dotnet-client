using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Rivr.Models.Authentication;
using Shouldly;

namespace Rivr.Test;

public class ErrorHandlingTests
{
    private Config _config = null!;
    private readonly Guid _merchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "clientSecret");
    }

    [Test]
    public async Task ApiCall_WhenUnauthorized_ThrowsUnauthorizedException()
    {
        // Arrange
        var authHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.Unauthorized);
        var apiHandler = new MockHttpMessageHandler();
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetDevicesAsync());
    }

    [Test]
    public async Task ApiCall_WhenForbidden_ThrowsForbiddenException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };
        var errorResponse = new ErrorResponse
        {
            Error = "forbidden",
            ErrorDescription = "Access denied to this resource"
        };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(errorResponse, System.Net.HttpStatusCode.Forbidden);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert
        var exception = await Should.ThrowAsync<ForbiddenException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetDevicesAsync());

        exception.Error.ShouldBe("forbidden");
        exception.ErrorDescription.ShouldBe("Access denied to this resource");
    }

    [Test]
    public async Task ApiCall_WhenServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var authResponse = new TokenResponse { ExpiresIn = 3600 };

        var authHandler = new MockHttpMessageHandler(authResponse);
        var apiHandler = new MockHttpMessageHandler(null, System.Net.HttpStatusCode.InternalServerError);
        var webhookHandler = new MockHttpMessageHandler();

        var authHttpClient = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);
        var webhookHttpClient = new HttpClient(webhookHandler);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, _config, memoryCache);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetDevicesAsync());
    }

    [Test]
    public async Task ApiCall_WhenNotFound_ThrowsHttpRequestException()
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

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await client
                .AsOrOnBehalfOfMerchant(_merchantId)
                .GetOrderAsync(Guid.NewGuid()));
    }
}
