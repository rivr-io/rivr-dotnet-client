namespace Rivr.Core.Models.Heartbeats;

/// <summary>
/// Represents a request to send a heartbeat.
/// </summary>
public class SendHeartbeatRequest
{
    /// <summary>
    /// Gets or sets the unique service ID.
    /// </summary>
    public string? UniqueServiceId { get; set; }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the currently installed service version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the latest available version from the update server.
    /// </summary>
    public string? LatestAvailableVersion { get; set; }
}
