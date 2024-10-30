using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

/// <summary>
/// Represents a token request.
/// </summary>
public abstract class TokenRequest
{
    /// <summary>
    /// The grant type.
    /// </summary>
    [JsonPropertyName("grant_type")]
    public abstract string GrantType { get; }
}