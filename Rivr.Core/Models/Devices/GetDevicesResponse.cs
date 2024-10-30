namespace Rivr.Core.Models.Devices;

/// <summary>
/// Represents the response from the GetDevices operation.
/// </summary>
public class GetDevicesResponse
{
    /// <summary>
    /// Gets or sets the devices.
    /// </summary>
    public Device[] Devices { get; set; } = [];
}