namespace Rivr.Core.Models.Setup;

/// <summary>
/// Represents the Rivr client builder.
/// </summary>
public interface IRivrClientBuilder
{
    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    string ClientId { set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    string ClientSecret { set; }

    /// <summary>
    /// Gets or sets the environment.
    /// </summary>
    Environment? Environment { set; }
}