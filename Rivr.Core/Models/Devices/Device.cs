using System;

namespace Rivr.Core.Models.Devices;

/// <summary>
/// Represents a device that belongs to a merchant.
/// </summary>
public class Device
{
    /// <summary>
    /// The ID of the device.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique identifier of the device.
    /// </summary>
    public string DeviceUniqueId { get; set; }

    /// <summary>
    /// The name of the device.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates whether the device is online.
    /// </summary>
    public bool IsOnline { get; set; }
}