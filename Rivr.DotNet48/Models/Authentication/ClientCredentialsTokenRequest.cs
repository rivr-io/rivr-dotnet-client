using Newtonsoft.Json;

namespace Rivr.DotNet48.Models.Authentication;

/// <inheritdoc />
public class ClientCredentialsTokenRequest(string clientId, string clientSecret) : TokenRequest
{
    /// <inheritdoc />
    public override string GrantType => "client_credentials";

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
}