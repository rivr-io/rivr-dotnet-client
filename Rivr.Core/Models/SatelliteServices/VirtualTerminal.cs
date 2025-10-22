using System;

namespace Rivr.Core.Models.SatelliteServices;

public class VirtualTerminal
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string POIID { get; set; } = string.Empty;
    public string POISerialNumber { get; set; } = string.Empty;
    public string TerminalEnvironment { get; set; } = "Attended";
    public string[] AcceptedSaleIDs { get; set; } = Array.Empty<string>();
    public Guid MerchantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int? Version { get; set; }
    public AssignedToDevice? AssignedToDevice { get; set; }
}

public class AssignedToDevice
{
    public Guid DeviceId { get; set; }
    public string? DeviceUniqueId { get; set; }
    public string? MerchantName { get; set; }
}

