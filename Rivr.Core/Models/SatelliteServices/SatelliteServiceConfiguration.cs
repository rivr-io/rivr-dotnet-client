namespace Rivr.Core.Models.SatelliteServices;

/// <summary>
/// Configuration settings for a satellite service.
/// </summary>
public class SatelliteServiceConfiguration
{
    /// <summary>
    /// Whether the subscription service is enabled.
    /// </summary>
    public bool SubscriptionServiceEnabled { get; set; } = true;

    /// <summary>
    /// The interval in minutes for the subscription service polling.
    /// </summary>
    public int? SubscriptionServiceIntervalInMinutes { get; set; }

    /// <summary>
    /// The configuration version number.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Target version for the service to update to.
    /// If null or empty, the service will update to the latest available version.
    /// If set to a specific version (e.g., "1.1.16"), the service will only update to that version.
    /// </summary>
    public string? TargetVersion { get; set; }
}
