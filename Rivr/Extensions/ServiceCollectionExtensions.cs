using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rivr.Core;
using Rivr.Core.Constants;
using Rivr.Core.Models.Setup;
using Environment = Rivr.Core.Models.Environment;

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
            client.BaseAddress = rivrClientBuilder.Environment == Environment.Production
                ? new Uri(ClientConfig.AuthBaseUri)
                : new Uri(ClientConfig.AuthBaseUriTest);
        });

        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = rivrClientBuilder.Environment == Environment.Production
                ? new Uri(ClientConfig.ApiBaseUri)
                : new Uri(ClientConfig.ApiBaseUriTest);
        });

        services.AddSingleton<IClient>(provider =>
        {
            var memoryCache = provider.GetRequiredService<IMemoryCache>();
            var options = provider.GetService<RivrClientOptions>();
            if (options != null)
            {
                rivrClientBuilder.UseOptions(options);
            }

            var authHttpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("AuthClient");
            var apiHttpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient");

            var config = rivrClientBuilder.ToConfig();

            return new Client(authHttpClient, apiHttpClient, config, memoryCache);
        });
        return services;
    }
}