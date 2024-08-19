using Microsoft.Extensions.DependencyInjection;

namespace Rivr.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivr client.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection Addclient(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<IClient, Client>();
        return services;
    }
}