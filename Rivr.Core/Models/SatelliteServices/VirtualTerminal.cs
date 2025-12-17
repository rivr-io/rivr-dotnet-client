using System;

namespace Rivr.Core.Models.SatelliteServices;

/// <summary>
/// Represents a virtual terminal for payment processing.
/// </summary>
public class VirtualTerminal
{
    /// <summary>
    /// The unique identifier of the virtual terminal.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The display name of the terminal.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The Point of Interaction ID (POIID) for the terminal.
    /// </summary>
    public string? POIID { get; set; }

    /// <summary>
    /// The serial number of the Point of Interaction.
    /// </summary>
    public string? POISerialNumber { get; set; }

    /// <summary>
    /// The terminal environment type (e.g., "Attended", "Unattended").
    /// </summary>
    public string? TerminalEnvironment { get; set; }

    /// <summary>
    /// Array of accepted Sale IDs for this terminal.
    /// </summary>
    public string[] AcceptedSaleIDs { get; set; } = [];

    /// <summary>
    /// The ID of the merchant that owns this terminal.
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Whether the terminal is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The date and time when the terminal was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// The date and time when the terminal was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// The version number of the terminal configuration.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Information about the device this terminal is assigned to, if any.
    /// </summary>
    public AssignedToDevice? AssignedToDevice { get; set; }
}

/// <summary>
/// Information about a device that a terminal is assigned to.
/// </summary>
public class AssignedToDevice
{
    /// <summary>
    /// The unique identifier of the device.
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// The unique device identifier string.
    /// </summary>
    public string? DeviceUniqueId { get; set; }

    /// <summary>
    /// The name of the merchant associated with the device.
    /// </summary>
    public string? MerchantName { get; set; }
}
