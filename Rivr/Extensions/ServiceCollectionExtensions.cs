using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rivr.Models.Setup;

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
    /// <param name="configuration"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddRivrClient(this IServiceCollection services, IConfiguration configuration, Action<IRivrClientBuilder> builder)
    {
        var rivrClientOptions = configuration.GetSection("Rivr").Get<RivrClientOptions>();

        if (rivrClientOptions != null)
        {
            services.AddSingleton(rivrClientOptions);
        }

        var rivrClientBuilder = new RivrClientBuilder();
        builder(rivrClientBuilder);

        services.AddMemoryCache();

        services.AddHttpClient("AuthClient", client =>
        {
            client.BaseAddress = rivrClientBuilder.Environment == Models.Environment.Production
                ? new Uri(Constants.ClientConfig.AuthBaseUri)
                : new Uri(Constants.ClientConfig.AuthBaseUriTest);
        });

        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = rivrClientBuilder.Environment == Models.Environment.Production
                ? new Uri(Constants.ClientConfig.ApiBaseUri)
                : new Uri(Constants.ClientConfig.ApiBaseUriTest);
        });

        services.AddSingleton<IClient>(provider =>
        {
            var memoryCache = provider.GetRequiredService<IMemoryCache>();
            var options = provider.GetService<RivrClientOptions>();
            rivrClientBuilder.UseOptions(options);

            var authHttpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("AuthClient");
            var apiHttpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient");

            var config = rivrClientBuilder.ToConfig();

            return new Client(authHttpClient, apiHttpClient, config, memoryCache);
        });
        return services;
    }
}