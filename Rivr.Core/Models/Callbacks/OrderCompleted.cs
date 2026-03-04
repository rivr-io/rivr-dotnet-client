using System;
using System.Collections.Generic;
using Rivr.Core.Models.Orders;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents the order completed.
/// </summary>
public class OrderCompleted
{
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the completed date.
    /// </summary>
    public DateTime CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the order.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}