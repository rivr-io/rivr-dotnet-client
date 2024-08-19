using Rivr.Models;

namespace Rivr;

/// <summary>
/// Represents the Rivr API client configuration.
/// </summary>
/// <param name="clientId"></param>
/// <param name="clientSecret"></param>
public class Config(string? clientId, string? clientSecret)
{
    /// <summary>
    /// The client_id used to authenticate with the Rivr API.
    /// </summary>
    public string? ClientId { internal get; set; } = clientId;

    /// <summary>
    /// The client_secret used to authenticate with the Rivr API.
    /// </summary>
    public string? ClientSecret { internal get; set; } = clientSecret;

    /// <summary>
    /// The environment to use (Production is default).
    /// </summary>
    public Environment Environment { get; set; } = Environment.Production;

    /// <summary>
    /// The base URI for the authentication endpoint. (Has a default value)
    /// </summary>
    public string AuthBaseUri { get; set; } = Constants.ClientConfig.AuthBaseUri;

    /// <summary>
    /// The base URI for the authentication endpoint in the test environment. (Has a default value)
    /// </summary>
    public string AuthBaseUriTest { get; set; } = Constants.ClientConfig.AuthBaseUriTest;

    /// <summary>
    /// The base URI for the API endpoint. (Has a default value)
    /// </summary>
    public string ApiBaseUri { get; set; } = Constants.ClientConfig.ApiBaseUri;

    /// <summary>
    /// The base URI for the API endpoint in the test environment. (Has a default value)
    /// </summary>
    public string ApiBaseUriTest { get; set; } = Constants.ClientConfig.ApiBaseUriTest;
}