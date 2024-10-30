using Rivr.Core.Constants;

namespace Rivr.Core.Models;

/// <summary>
/// Represents the Rivr API client configuration.
/// </summary>
/// <param name="clientId"></param>
/// <param name="clientSecret"></param>
/// <param name="environment"></param>
public class Config(string clientId, string clientSecret, Environment? environment = Environment.Production)
{
    /// <summary>
    /// The client_id used to authenticate with the Rivr API.
    /// </summary>
    public string ClientId { get; set; } = clientId;

    /// <summary>
    /// The client_secret used to authenticate with the Rivr API.
    /// </summary>
    public string ClientSecret { get; set; } = clientSecret;

    /// <summary>
    /// The environment to use (Production is default).
    /// </summary>
    public Environment Environment { get; set; } = environment ?? Environment.Production;

    /// <summary>
    /// The base URI for the authentication endpoint. (Has a default value)
    /// </summary>
    public string AuthBaseUri { get; set; } = ClientConfig.AuthBaseUri;

    /// <summary>
    /// The base URI for the authentication endpoint in the test environment. (Has a default value)
    /// </summary>
    public string AuthBaseUriTest { get; set; } = ClientConfig.AuthBaseUriTest;

    /// <summary>
    /// The base URI for the API endpoint. (Has a default value)
    /// </summary>
    public string ApiBaseUri { get; set; } = ClientConfig.ApiBaseUri;

    /// <summary>
    /// The base URI for the API endpoint in the test environment. (Has a default value)
    /// </summary>
    public string ApiBaseUriTest { get; set; } = ClientConfig.ApiBaseUriTest;
}