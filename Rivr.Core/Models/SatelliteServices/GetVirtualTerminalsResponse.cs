using System;

namespace Rivr.Core.Models.SatelliteServices;

public class GetVirtualTerminalsResponse
{
    public VirtualTerminal[] VirtualTerminals { get; set; } = Array.Empty<VirtualTerminal>();
}

