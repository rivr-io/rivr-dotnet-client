using System.Text.Json.Serialization;

namespace Rivr.Core.Models;

/// <summary>
/// Represents an error response from the API.
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

    /// <summary>
    /// The API's stable error code, for example <c>invalid_scope</c>, if the response carried one. Every error
    /// answer under <c>/api/public</c> has carried it since 2026-10-25.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// The address of the page that explains the error, <c>https://developers.rivr.io/errors/{code}</c>, if the
    /// response carried one.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
