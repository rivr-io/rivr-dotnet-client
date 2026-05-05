namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents a postal address.
/// </summary>
public class Address
{
    /// <summary>
    /// The street address (line 1).
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// The street address (line 2).
    /// </summary>
    public string? Street2 { get; set; }

    /// <summary>
    /// The postal/zip code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// The city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// The ISO 3166-1 alpha-2 country code (e.g. "SE", "NO").
    /// </summary>
    public string? Country { get; set; }
}
