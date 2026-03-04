using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Constants;
using Rivr.Core.Models;
using Shouldly;
using RivrEnvironment = Rivr.Core.Models.Environment;

namespace Rivr.Test;

public class ConfigUriTests
{
    [Test]
    public void Config_DefaultUris_AreCorrectForProduction()
    {
        // Arrange & Act
        var config = new Config(clientId: "id", clientSecret: "secret");

        // Assert
        config.AuthBaseUri.ShouldBe(ClientConfig.AuthBaseUri);
        config.ApiBaseUri.ShouldBe(ClientConfig.ApiBaseUri);
        config.WebhookAggregatorBaseUri.ShouldBe(ClientConfig.WebhookAggregatorBaseUri);
    }

    [Test]
    public void Config_DefaultUris_AreCorrectForTest()
    {
        // Arrange & Act
        var config = new Config(clientId: "id", clientSecret: "secret");

        // Assert
        config.AuthBaseUriTest.ShouldBe(ClientConfig.AuthBaseUriTest);
        config.ApiBaseUriTest.ShouldBe(ClientConfig.ApiBaseUriTest);
        config.WebhookAggregatorBaseUriTest.ShouldBe(ClientConfig.WebhookAggregatorBaseUriTest);
    }

    [Test]
    public void Config_CustomAuthBaseUri_OnlyAffectsProduction()
    {
        // Arrange & Act
        var config = new Config(
            clientId: "id",
            clientSecret: "secret",
            authBaseUri: "https://custom-auth.example.com/"
        );

        // Assert
        config.AuthBaseUri.ShouldBe("https://custom-auth.example.com/");
        config.AuthBaseUriTest.ShouldBe(ClientConfig.AuthBaseUriTest);
    }

    [Test]
    public void Config_CustomApiBaseUri_OnlyAffectsProduction()
    {
        // Arrange & Act
        var config = new Config(
            clientId: "id",
            clientSecret: "secret",
            apiBaseUri: "https://custom-api.example.com/"
        );

        // Assert
        config.ApiBaseUri.ShouldBe("https://custom-api.example.com/");
        config.ApiBaseUriTest.ShouldBe(ClientConfig.ApiBaseUriTest);
    }

    [Test]
    public void Config_CustomWebhookAggregatorBaseUri_OnlyAffectsProduction()
    {
        // Arrange & Act
        var config = new Config(
            clientId: "id",
            clientSecret: "secret",
            webhookAggregatorBaseUri: "https://custom-webhook.example.com/"
        );

        // Assert
        config.WebhookAggregatorBaseUri.ShouldBe("https://custom-webhook.example.com/");
        config.WebhookAggregatorBaseUriTest.ShouldBe(ClientConfig.WebhookAggregatorBaseUriTest);
    }

    [Test]
    public void Client_WhenClientIdWithoutSecret_ThrowsArgumentException()
    {
        // Arrange
        var config = new Config(clientId: "id", clientSecret: "");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new Client(config, memoryCache));

        exception.Message.ShouldContain("ClientSecret must be provided");
    }

    [Test]
    public void Client_WhenMerchantIdWithoutSecret_ThrowsArgumentException()
    {
        // Arrange
        var config = new Config(clientId: "", clientSecret: "", merchantId: "merchant-id", merchantSecret: "");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new Client(config, memoryCache));

        exception.Message.ShouldContain("MerchantSecret must be provided");
    }

    [Test]
    public void Config_UniqueServiceId_IsSetCorrectly()
    {
        // Arrange & Act
        var config = new Config(
            clientId: "id",
            clientSecret: "secret",
            uniqueServiceId: "my-service-001"
        );

        // Assert
        config.UniqueServiceId.ShouldBe("my-service-001");
    }

    [Test]
    public void Config_UniqueServiceId_IsNullByDefault()
    {
        // Arrange & Act
        var config = new Config(clientId: "id", clientSecret: "secret");

        // Assert
        config.UniqueServiceId.ShouldBeNull();
    }
}
