namespace Rivr.Core.Constants;

/// <summary>
/// Represents the client configuration.
/// </summary>
public class ClientConfig
{
    /// <summary>
    /// The base URI for the authentication service.
    /// </summary>
    public const string AuthBaseUri = "https://auth.rivr.io/";

    /// <summary>
    /// The base URI for the API.
    /// </summary>
    public const string ApiBaseUri = "https://api.rivr.io/api/public/";

    /// <summary>
    /// The base URI for the test authentication service.
    /// </summary>
    public const string AuthBaseUriTest = "https://auth.test.rivr.io/";

    /// <summary>
    /// The base URI for the test API.
    /// </summary>
    public const string ApiBaseUriTest = "https://api.test.rivr.io/api/public/";

    /// <summary>
    /// The base URI for the Webhook Aggregator service.
    /// </summary>
    public const string WebhookAggregatorBaseUri = "https://webhook-aggregator.rivr.io/";

    /// <summary>
    /// The base URI for the test Webhook Aggregator service.
    /// </summary>
    public const string WebhookAggregatorBaseUriTest = "https://webhook-aggregator.test.rivr.io/";
}