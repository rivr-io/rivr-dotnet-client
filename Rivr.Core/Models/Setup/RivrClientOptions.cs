namespace Rivr.Core.Models.Setup;

/// <summary>
/// Represents the Rivr client options.
/// </summary>
public class RivrClientOptions
{
    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the merchant ID.
    /// </summary>
    public string MerchantId { get; set; }

    /// <summary>
    /// Gets or sets the merchant secret.
    /// </summary>
    public string MerchantSecret { get; set; }

    /// <summary>
    /// Gets or sets the environment.
    /// </summary>
    public Environment? Environment { get; set; }

    /// <summary>
    /// Gets or sets the API base URI for authentication.
    /// </summary>
    public string AuthBaseUri { get; set; }

    /// <summary>
    /// Gets or sets the API base URI for the Rivr API.
    /// </summary>
    public string ApiBaseUri { get; set; }

    /// <summary>
    /// Gets or sets the Webhook Aggregator base URI.
    /// </summary>
    public string WebhookAggregatorBaseUri { get; set; }
}