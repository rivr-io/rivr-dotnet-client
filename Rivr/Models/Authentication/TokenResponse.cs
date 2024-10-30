using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

/// <summary>
/// Represents a token response.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// The token type.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    /// <summary>
    /// The amount of time in seconds until the token expires.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The scope that the token is valid for.
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}