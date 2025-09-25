using System;
using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

/// <summary>
/// Merchant credentials request.
/// </summary>
public class MerchantCredentialsRequest : TokenRequest
{
    /// <summary>
    /// Merchant credentials request.
    /// </summary>
    /// <param name="merchantId"></param>
    /// <param name="merchantSecret"></param>
    public MerchantCredentialsRequest(string merchantId, string merchantSecret)
    {
        MerchantId = Guid.Parse(merchantId);
        MerchantSecret = merchantSecret;
    }

    /// <summary>
    /// The grant type.
    /// </summary>
    public override string GrantType => GrantTypes.MerchantCredentials;

    /// <summary>
    /// The merchant ID.
    /// </summary>
    [JsonPropertyName("merchant_id")]
    public Guid MerchantId { get; set; }

    /// <summary>
    /// The merchant secret.
    /// </summary>
    [JsonPropertyName("merchant_secret")]
    public string MerchantSecret { get; set; }
}