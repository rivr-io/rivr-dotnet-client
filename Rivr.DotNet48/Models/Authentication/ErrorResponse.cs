using Newtonsoft.Json;

namespace Rivr.DotNet48.Models.Authentication;

/// <summary>
/// Represents an authentication error response from the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The error code.
    /// </summary>
    [JsonProperty("error")]
    public string Error { get; set; }

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    [JsonProperty("error_description")]
    public string ErrorDescription { get; set; }
}