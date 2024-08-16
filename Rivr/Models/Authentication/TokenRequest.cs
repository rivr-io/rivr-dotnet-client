using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

internal abstract class TokenRequest
{
    [JsonPropertyName("grant_type")]
    public abstract string GrantType { get; }
}