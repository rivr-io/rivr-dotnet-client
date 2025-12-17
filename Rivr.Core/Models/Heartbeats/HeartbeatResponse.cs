using Rivr.Core.Models.SatelliteServices;

namespace Rivr.Core.Models.Heartbeats;

/// <summary>
/// Response from a heartbeat request containing service configuration.
/// </summary>
public class HeartbeatResponse
{
    /// <summary>
    /// The satellite service configuration returned by the server.
    /// </summary>
    public SatelliteServiceConfiguration? Configuration { get; set; }
}
