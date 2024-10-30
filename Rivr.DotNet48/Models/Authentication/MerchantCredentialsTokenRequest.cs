using System;
using Newtonsoft.Json;

namespace Rivr.DotNet48.Models.Authentication;

/// <summary>
/// Represents a token request for merchant credentials.
/// </summary>
/// <param name="clientId"></param>
/// <param name="clientSecret"></param>
/// <param name="merchantId"></param>
public class MerchantCredentialsTokenRequest(string clientId, string clientSecret, Guid merchantId) : TokenRequest
{
    /// <summary>
    /// The grant type for merchant credentials.
    /// </summary>
    public override string GrantType => "merchant_credentials";

    /// <summary>
    /// The client ID.
    /// </summary>
    [JsonProperty("client_id")]
    public string ClientId { get; set; } = clientId;

    /// <summary>
    /// The client secret.
    /// </summary>
    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; } = clientSecret;

    /// <summary>
    /// The merchant ID.
    /// </summary>
    [JsonProperty("merchant_id")]
    public Guid MerchantId { get; set; } = merchantId;
}