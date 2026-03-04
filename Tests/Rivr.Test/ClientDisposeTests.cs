using Microsoft.Extensions.Caching.Memory;
using Rivr.Core.Models;
using Shouldly;

namespace Rivr.Test;

public class ClientDisposeTests
{
    [Test]
    public void Dispose_DisposesHttpClients()
    {
        // Arrange
        var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var authHttpClient = new HttpClient();
        var apiHttpClient = new HttpClient();
        var webhookHttpClient = new HttpClient();

        var client = new Client(authHttpClient, apiHttpClient, webhookHttpClient, config, memoryCache);

        // Act
        client.Dispose();

        // Assert - Disposed HttpClients throw ObjectDisposedException on use
        Should.Throw<ObjectDisposedException>(() => authHttpClient.GetAsync("http://localhost").GetAwaiter().GetResult());
        Should.Throw<ObjectDisposedException>(() => apiHttpClient.GetAsync("http://localhost").GetAwaiter().GetResult());
        Should.Throw<ObjectDisposedException>(() => webhookHttpClient.GetAsync("http://localhost").GetAwaiter().GetResult());
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var client = new Client(config, memoryCache);

        // Act & Assert - Should not throw
        client.Dispose();
        client.Dispose();
    }

    [Test]
    public void Client_ImplementsIDisposable()
    {
        // Arrange
        var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Act & Assert
        using var client = new Client(config, memoryCache);
        client.ShouldBeAssignableTo<IDisposable>();
    }

    [Test]
    public void Client_WorksWithUsingStatement()
    {
        // Arrange
        var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        Client? clientRef;

        // Act
        using (var client = new Client(config, memoryCache))
        {
            clientRef = client;
            client.ShouldNotBeNull();
        }

        // Assert - Client has been disposed after using block
        clientRef.ShouldNotBeNull();
    }
}
