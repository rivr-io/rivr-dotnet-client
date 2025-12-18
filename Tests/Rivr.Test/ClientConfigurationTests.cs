using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Shouldly;
using RivrEnvironment = Rivr.Core.Models.Environment;

namespace Rivr.Test;

public class ClientConfigurationTests
{
    [Test]
    public void Client_WhenBothClientIdAndMerchantId_ThrowsArgumentException()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var config = new Config(
            clientId: "client-id",
            clientSecret: "client-secret",
            merchantId: "merchant-id",
            merchantSecret: "merchant-secret"
        );

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new Client(config, memoryCache));

        exception.Message.ShouldContain("one of");
    }

    [Test]
    public void Client_WhenValidClientCredentials_CreatesSuccessfully()
    {
        // Arrange
        var config = new Config(clientId: "client-id", clientSecret: "client-secret");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Act
        var client = new Client(config, memoryCache);

        // Assert
        client.ShouldNotBeNull();
        client.IsConfiguredForSingleMerchant.ShouldBeFalse();
    }

    [Test]
    public void Client_WhenValidMerchantCredentials_CreatesSuccessfully()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var config = new Config(
            clientId: "",
            clientSecret: "",
            merchantId: merchantId.ToString(),
            merchantSecret: "merchant-secret"
        );
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Act
        var client = new Client(config, memoryCache);

        // Assert
        client.ShouldNotBeNull();
        client.IsConfiguredForSingleMerchant.ShouldBeTrue();
        client.ConfiguredMerchantId.ShouldBe(merchantId);
    }

    [Test]
    public void Client_WhenNotConfiguredForSingleMerchant_ThrowsOnConfiguredMerchantId()
    {
        // Arrange
        var config = new Config(clientId: "client-id", clientSecret: "client-secret");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var client = new Client(config, memoryCache);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
            _ = client.ConfiguredMerchantId);

        exception.Message.ShouldContain("not configured with Merchant Credentials");
    }

    [Test]
    public void Client_WhenEmptyClientIdAndMerchantId_ThrowsArgumentException()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var config = new Config(clientId: "", clientSecret: "");

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new Client(config, memoryCache));

        exception.Message.ShouldContain("ClientId or MerchantId must be provided");
    }

    [Test]
    public void Config_SetsEnvironmentToProduction_ByDefault()
    {
        // Arrange & Act
        var config = new Config(clientId: "client-id", clientSecret: "client-secret");

        // Assert
        config.Environment.ShouldBe(RivrEnvironment.Production);
    }

    [Test]
    public void Config_SetsEnvironmentToTest_WhenSpecified()
    {
        // Arrange & Act
        var config = new Config(
            clientId: "client-id",
            clientSecret: "client-secret",
            environment: RivrEnvironment.Test
        );

        // Assert
        config.Environment.ShouldBe(RivrEnvironment.Test);
    }
}
