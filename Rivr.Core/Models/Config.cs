using Rivr.Core.Constants;

namespace Rivr.Core.Models;

/// <summary>
/// Represents the Rivr API client configuration.
/// </summary>
/// <param name="clientId"></param>
/// <param name="clientSecret"></param>
/// <param name="environment"></param>
public class Config(
    string clientId,
    string clientSecret,
    string merchantId = null,
    string merchantSecret = null,
    string authBaseUri = null,
    string apiBaseUri = null,
    string webhookAggregatorBaseUri = null,
    string uniqueServiceId = null,
    Environment? environment = Environment.Production)
{
    /// <summary>
    /// The client_id used to authenticate with the Rivr API.
    /// </summary>
    public string ClientId { get; set; } = clientId;

    /// <summary>
    /// The client_secret used to authenticate with the Rivr API.
    /// </summary>
    public string ClientSecret { get; set; } = clientSecret;

    /// <summary>
    /// The merchant_id to authenticate with the Rivr API.
    /// </summary>
    public string MerchantId { get; set; } = merchantId;

    /// <summary>
    /// The merchant_secret to authenticate with the Rivr API.
    /// </summary>
    public string MerchantSecret { get; set; } = merchantSecret;

    /// <summary>
    /// The environment to use (Production is default).
    /// </summary>
    public Environment Environment { get; set; } = environment ?? Environment.Production;

    /// <summary>
    /// The base URI for the authentication endpoint. (Has a default value)
    /// </summary>
    public string AuthBaseUri { get; set; } = authBaseUri ?? ClientConfig.AuthBaseUri;

    /// <summary>
    /// The base URI for the authentication endpoint in the test environment. (Has a default value)
    /// </summary>
    public string AuthBaseUriTest { get; set; } = authBaseUri ?? ClientConfig.AuthBaseUriTest;

    /// <summary>
    /// The base URI for the API endpoint. (Has a default value)
    /// </summary>
    public string ApiBaseUri { get; set; } = apiBaseUri ?? ClientConfig.ApiBaseUri;

    /// <summary>
    /// The base URI for the API endpoint in the test environment. (Has a default value)
    /// </summary>
    public string ApiBaseUriTest { get; set; } = apiBaseUri ?? ClientConfig.ApiBaseUriTest;

    /// <summary>
    /// The base URI for the Webhook Aggregator endpoint. (Has a default value)
    /// </summary>
    public string WebhookAggregatorBaseUri { get; set; } = webhookAggregatorBaseUri ?? ClientConfig.WebhookAggregatorBaseUri;

    /// <summary>
    /// The base URI for the Webhook Aggregator endpoint in the test environment. (Has a default value)
    /// </summary>
    public string WebhookAggregatorBaseUriTest { get; set; } = webhookAggregatorBaseUri ?? ClientConfig.WebhookAggregatorBaseUriTest;
}