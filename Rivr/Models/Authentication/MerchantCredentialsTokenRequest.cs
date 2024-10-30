using System;
using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

/// <summary>
/// Merchant credentials token request.
/// </summary>
/// <param name="clientId"></param>
/// <param name="clientSecret"></param>
/// <param name="merchantId"></param>
public class MerchantCredentialsTokenRequest(string clientId, string clientSecret, Guid merchantId) : TokenRequest
{
    /// <summary>
    /// The grant type.
    /// </summary>
    public override string GrantType => "merchant_credentials";

    /// <summary>
    /// The client ID.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = clientId;

    /// <summary>
    /// The client secret.
    /// </summary>
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = clientSecret;

    /// <summary>
    /// The merchant ID.
    /// </summary>
    [JsonPropertyName("merchant_id")]
    public Guid MerchantId { get; set; } = merchantId;
}