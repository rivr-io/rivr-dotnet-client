using System;
using System.Collections.Generic;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents the order refunded.
/// </summary>
public class OrderRefunded
{
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the refunded date.
    /// </summary>
    public DateTime RefundedDate { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the order.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}