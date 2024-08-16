using System;
using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

internal class MerchantCredentialsTokenRequest(string clientId, string clientSecret, Guid merchantId) : TokenRequest
{
    public override string GrantType => "merchant_credentials";

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = clientId;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = clientSecret;

    [JsonPropertyName("merchant_id")]
    public Guid MerchantId { get; set; } = merchantId;
}