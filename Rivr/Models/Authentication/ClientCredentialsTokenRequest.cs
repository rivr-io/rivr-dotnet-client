using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

internal class ClientCredentialsTokenRequest(string clientId, string clientSecret) : TokenRequest
{
    public override string GrantType => "client_credentials";

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = clientId;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = clientSecret;
}