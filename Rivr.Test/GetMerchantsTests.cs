using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Rivr.Models.Authentication;
using Rivr.Models.Merchants;
using Shouldly;

namespace Rivr.Test;

public class GetMerchantsTests
{
    private Config _config;

    [SetUp]
    public void Setup()
    {
        _config = new Config(clientId: "clientId", clientSecret: "client");
    }

    [Test]
    public async Task ShouldReturnMerchants()
    {
        // Arrange
        var authReponse = new TokenResponse();
        var merchantsResponse = new GetMerchantsResponse
        {
            Merchants = [new Merchant { Name = "Merchant 1" }],
            Metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1,
                TotalPages = 1
            }
        };

        var authHandler = new MockHttpMessageHandler(authReponse);
        var apiHandler = new MockHttpMessageHandler(merchantsResponse);

        var authHttpHandler = new HttpClient(authHandler);
        var apiHttpClient = new HttpClient(apiHandler);

        var memoryCache = Substitute.For<IMemoryCache>();

        var client = new Client(authHttpHandler, apiHttpClient, _config, memoryCache);

        // Act
        var result = await client
            .AsPlatform()
            .GetMerchantsAsync();

        // Assert
        authHandler.PerformedRequestsCount.ShouldBe(1);
        apiHandler.PerformedRequestsCount.ShouldBe(1);

        var authRequestContent = authHandler.GetRequestContent<ClientCredentialsTokenRequest>();
        authRequestContent.ShouldNotBeNull();
        authRequestContent.GrantType.ShouldBe("client_credentials");

        result.ShouldNotBeNull();
        result.Merchants.Length.ShouldBe(1);
        result.Merchants.First().Name.ShouldBe("Merchant 1");
    }
}