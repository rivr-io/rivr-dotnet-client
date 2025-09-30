using Rivr.Core.Models.Merchants;

namespace Rivr.Core.Models.Setup;

/// <summary>
/// Represents the Rivr client builder.
/// </summary>
public interface IRivrClientBuilder
{
    /// <summary>
    /// Gets the client secret.
    /// </summary>
    string ClientId { set; }

    /// <summary>
    /// Gets the client secret.
    /// </summary>
    string ClientSecret { set; }


    /// <summary>
    /// The merchant_id to authenticate with the Rivr API.
    /// </summary>
    public string MerchantId { set; }

    /// <summary>
    /// The merchant_secret to authenticate with the Rivr API.
    /// </summary>
    public string MerchantSecret { set; }

    /// <summary>
    /// Gets the environment.
    /// </summary>
    Environment? Environment { set; }

    /// <summary>
    /// Gets the API base URI for authentication.
    /// There is a default value based on Environment if not set.
    /// </summary>
    public string AuthBaseUri { get; }

    /// <summary>
    /// Gets the API base URI for the Rivr API.
    /// There is a default value based on Environment if not set.
    /// </summary>
    public string ApiBaseUri { get; }

    /// <summary>
    /// Gets the Webhook Aggregator base URI.
    /// There is a default value based on Environment if not set.
    /// </summary>
    public string WebhookAggregatorBaseUri { get; }
}