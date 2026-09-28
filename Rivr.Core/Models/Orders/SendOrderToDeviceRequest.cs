using System;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// The request body for <see cref="Rivr.Core.IMerchantOperations.SendOrderToDeviceAsync"/>.
/// </summary>
public class SendOrderToDeviceRequest
{
    /// <summary>
    /// The device to send the order to (<see cref="Devices.Device.Id"/>).
    /// </summary>
    public Guid DeviceId { get; set; }
}
