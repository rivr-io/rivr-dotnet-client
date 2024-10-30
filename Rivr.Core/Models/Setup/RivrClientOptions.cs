namespace Rivr.Core.Models.Setup;

/// <summary>
/// Represents the Rivr client options.
/// </summary>
public class RivrClientOptions
{
    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the environment.
    /// </summary>
    public Environment? Environment { get; set; }
}