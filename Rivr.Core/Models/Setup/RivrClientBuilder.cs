namespace Rivr.Core.Models.Setup;

/// <summary>
/// Represents the Rivr client configurator.
/// </summary>
public class RivrClientBuilder : IRivrClientBuilder
{
    /// <inheritdoc />
    public string ClientId { internal get; set; }

    /// <inheritdoc />
    public string ClientSecret { internal get; set; }

    /// <inheritdoc />
    public Environment? Environment { get; set; }

    /// <inheritdoc />
    public string AuthBaseUri { get; set; }

    /// <inheritdoc />
    public string ApiBaseUri { get; set; }

    /// <inheritdoc />
    public string WebhookAggregatorBaseUri { get; set; }

    /// <summary>
    /// Creates an instance of <see cref="Config"/>.
    /// </summary>
    /// <returns></returns>
    public Config ToConfig()
    {
        return new Config(ClientId, ClientSecret, AuthBaseUri, ApiBaseUri, WebhookAggregatorBaseUri, Environment);
    }

    /// <summary>
    /// Uses the options.
    /// </summary>
    /// <param name="rivrClientOptions"></param>
    public void UseOptions(RivrClientOptions rivrClientOptions)
    {
        if (rivrClientOptions == null)
        {
            return;
        }

        ClientId ??= rivrClientOptions.ClientId;
        ClientSecret ??= rivrClientOptions.ClientSecret;
        Environment ??= rivrClientOptions.Environment;
        AuthBaseUri ??= rivrClientOptions.AuthBaseUri;
        ApiBaseUri ??= rivrClientOptions.ApiBaseUri;
        WebhookAggregatorBaseUri ??= rivrClientOptions.WebhookAggregatorBaseUri;
    }
}