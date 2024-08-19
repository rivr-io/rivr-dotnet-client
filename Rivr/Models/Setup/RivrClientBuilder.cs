namespace Rivr.Models.Setup;

/// <summary>
/// Represents the Rivr client configurator.
/// </summary>
public class RivrClientBuilder : IRivrClientBuilder
{
    /// <inheritdoc />
    public string? ClientId { internal get; set; }

    /// <inheritdoc />
    public string? ClientSecret { internal get; set; }

    /// <inheritdoc />
    public Environment? Environment { internal get; set; }

    internal Config ToConfig()
    {
        return new Config(ClientId, ClientSecret, Environment);
    }

    public void UseOptions(RivrClientOptions? rivrClientOptions)
    {
        if (rivrClientOptions == null)
        {
            return;
        }

        ClientId ??= rivrClientOptions.ClientId;
        ClientSecret ??= rivrClientOptions.ClientSecret;
        Environment ??= rivrClientOptions.Environment;
    }
}