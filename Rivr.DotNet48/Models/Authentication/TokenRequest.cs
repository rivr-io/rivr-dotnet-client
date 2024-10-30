using Newtonsoft.Json;

namespace Rivr.DotNet48.Models.Authentication;

/// <summary>
/// Represents a token request.
/// </summary>
public abstract class TokenRequest
{
    /// <summary>
    /// The grant type for the token request.
    /// </summary>
    [JsonProperty("grant_type")]
    public abstract string GrantType { get; }
}