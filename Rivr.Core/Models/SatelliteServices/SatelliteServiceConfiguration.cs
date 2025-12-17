namespace Rivr.Core.Models.SatelliteServices;

public class SatelliteServiceConfiguration
{
    public bool SubscriptionServiceEnabled { get; set; } = true;
    public int? SubscriptionServiceIntervalInMinutes { get; set; }
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Target version for the service to update to.
    /// If null or empty, the service will update to the latest available version.
    /// If set to a specific version (e.g., "1.1.16"), the service will only update to that version.
    /// </summary>
    public string? TargetVersion { get; set; }
}


