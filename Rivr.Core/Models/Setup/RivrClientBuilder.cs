namespace Rivr.Core.Models.Setup;

/// <summary>
/// Represents the Rivr client configurator.
/// </summary>
public class RivrClientBuilder : IRivrClientBuilder
{
    /// <inheritdoc />
    public string? ClientId { internal get; set; }

    /// <inheritdoc />
    public string? ClientSecret { internal get; set; }

    /// <inheritdoc />
    public string? MerchantId { internal get; set; }

    /// <inheritdoc />
    public string? MerchantSecret { get; set; }

    /// <inheritdoc />
    public Environment? Environment { get; set; }

    /// <inheritdoc />
    public string? AuthBaseUri { get; internal set; }

    /// <inheritdoc />
    public string? ApiBaseUri { get; internal set; }

    /// <inheritdoc />
    public string? WebhookAggregatorBaseUri { get; internal set; }

    /// <summary>
    /// Creates an instance of <see cref="Config"/>.
    /// </summary>
    /// <returns></returns>
    public Config ToConfig()
    {
        return new Config(
            ClientId,
            ClientSecret,
            MerchantId,
            MerchantSecret,
            authBaseUri: AuthBaseUri,
            apiBaseUri: ApiBaseUri,
            webhookAggregatorBaseUri: WebhookAggregatorBaseUri,
            environment: Environment);
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
        MerchantId ??= rivrClientOptions.MerchantId;
        MerchantSecret ??= rivrClientOptions.MerchantSecret;
        Environment ??= rivrClientOptions.Environment;
        AuthBaseUri ??= rivrClientOptions.AuthBaseUri;
        ApiBaseUri ??= rivrClientOptions.ApiBaseUri;
        WebhookAggregatorBaseUri ??= rivrClientOptions.WebhookAggregatorBaseUri;
    }
}