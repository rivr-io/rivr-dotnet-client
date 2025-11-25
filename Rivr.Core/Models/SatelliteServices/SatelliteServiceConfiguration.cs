namespace Rivr.Core.Models.SatelliteServices;

public class SatelliteServiceConfiguration
{
    public bool SubscriptionServiceEnabled { get; set; } = true;
    public int? SubscriptionServiceIntervalInMinutes { get; set; }
    public int Version { get; set; } = 1;
}


