using System;

namespace Rivr.Models.Merchants;

/// <summary>
/// Represents a merchant.
/// </summary>
public class Merchant
{
    /// <summary>
    /// The ID of the merchant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the merchant.
    /// </summary>
    public string? Name { get; set; }
}