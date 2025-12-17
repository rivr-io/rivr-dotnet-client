namespace Rivr.Core.Models.SatelliteServices;

/// <summary>
/// Response containing a list of virtual terminals.
/// </summary>
public class GetVirtualTerminalsResponse
{
    /// <summary>
    /// Array of virtual terminals.
    /// </summary>
    public VirtualTerminal[] VirtualTerminals { get; set; } = [];
}
