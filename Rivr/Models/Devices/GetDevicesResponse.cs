using System;

namespace Rivr.Models.Devices;

public class GetDevicesResponse
{
    public MerchantDevice[] Devices { get; set; } = [];
}