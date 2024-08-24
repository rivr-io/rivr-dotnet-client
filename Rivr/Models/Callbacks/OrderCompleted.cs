using System;
using Rivr.Models.Orders;

namespace Rivr.Models.Callbacks;

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
}