using Microsoft.Extensions.DependencyInjection;

namespace Rivr.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Addclient(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }
}