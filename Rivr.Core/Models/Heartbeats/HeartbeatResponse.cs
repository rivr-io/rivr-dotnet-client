using Rivr.Core.Models.SatelliteServices;

namespace Rivr.Core.Models.Heartbeats;

public class HeartbeatResponse
{
    public SatelliteServiceConfiguration Configuration { get; set; } = new();
}


