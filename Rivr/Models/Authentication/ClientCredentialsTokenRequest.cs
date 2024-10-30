using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

/// <summary>
/// Client credentials token request.
/// </summary>
/// <param name="clientId"></param>
/// <param name="clientSecret"></param>
public class ClientCredentialsTokenRequest(string clientId, string clientSecret) : TokenRequest
{
    /// <summary>
    /// The grant type.
    /// </summary>
    public override string GrantType => "client_credentials";

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
}