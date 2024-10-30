using System.Text.Json.Serialization;

namespace Rivr.Models.Authentication;

/// <summary>
/// Represents an authentication error response from the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The error code.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}