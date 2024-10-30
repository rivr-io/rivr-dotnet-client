using Rivr.Core.Models;
using Rivr.Core.Models.Setup;

namespace Rivr.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IRivrClientBuilder"/>.
/// </summary>
public static class RivrClientBuilderExtensions
{
    /// <summary>
    /// Sets the client ID.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public static IRivrClientBuilder UseClientId(this IRivrClientBuilder builder, string clientId)
    {
        builder.ClientId = clientId;
        return builder;
    }

    /// <summary>
    /// Sets the client secret.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="clientSecret"></param>
    /// <returns></returns>
    public static IRivrClientBuilder UseClientSecret(this IRivrClientBuilder builder, string clientSecret)
    {
        builder.ClientSecret = clientSecret;
        return builder;
    }

    /// <summary>
    /// Sets the environment.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    public static IRivrClientBuilder UseEnvironment(this IRivrClientBuilder builder, Environment environment = Environment.Production)
    {
        builder.Environment = environment;
        return builder;
    }

    /// <summary>
    /// Sets the environment to the test environment.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRivrClientBuilder UseTestEnvironment(this IRivrClientBuilder builder)
    {
        builder.UseEnvironment(Environment.Test);
        return builder;
    }

    /// <summary>
    /// Sets the environment to the production environment.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRivrClientBuilder UseProductionEnvironment(this IRivrClientBuilder builder)
    {
        builder.UseEnvironment(Environment.Production);
        return builder;
    }
}