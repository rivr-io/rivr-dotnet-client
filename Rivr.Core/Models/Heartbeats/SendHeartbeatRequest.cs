namespace Rivr.Core.Models.Heartbeats;

/// <summary>
/// Represents a request to send a heartbeat.
/// </summary>
public class SendHeartbeatRequest
{
    /// <summary>
    /// Gets or sets the unique service ID.
    /// </summary>
    public string UniqueServiceId { get; set; }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the service version.
    /// </summary>
    public string Version { get; set; }
}