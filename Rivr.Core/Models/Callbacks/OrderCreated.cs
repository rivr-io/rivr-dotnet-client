using System;
using System.Collections.Generic;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents the order created.
/// </summary>
public class OrderCreated
{
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the order.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}