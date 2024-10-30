using Newtonsoft.Json;

namespace Rivr.DotNet48.Models.Authentication;

/// <summary>
/// Represents a token response from the API.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The access token.
    /// </summary>
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    /// <summary>
    /// The token type.
    /// </summary>
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    /// <summary>
    /// The number of seconds until the token expires.
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The scope of the token.
    /// </summary>
    [JsonProperty("scope")]
    public string Scope { get; set; }
}