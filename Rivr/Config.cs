using Rivr.Models;

namespace Rivr;

public class Config(string? clientId, string? clientSecret)
{
    public string? ClientId { internal get; set; } = clientId;
    public string? ClientSecret { internal get; set; } = clientSecret;
    public Environment Environment { get; set; } = Environment.Production;
    public string AuthBaseUri { get; set; } = Constants.ClientConfig.AuthBaseUri;
    public string AuthBaseUriTest { get; set; } = Constants.ClientConfig.AuthBaseUriTest;
    public string ApiBaseUri { get; set; } = Constants.ClientConfig.ApiBaseUri;
    public string ApiBaseUriTest { get; set; } = Constants.ClientConfig.ApiBaseUriTest;
}