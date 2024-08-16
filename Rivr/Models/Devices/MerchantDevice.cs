using System;

namespace Rivr.Models.Devices;

public class MerchantDevice
{
    public Guid Id { get; set; }
    public string? DeviceUniqueId { get; set; }
    public string? Name { get; set; }
    public bool IsOnline { get; set; }
}