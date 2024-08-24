using System;
using Rivr.Models.Orders;

namespace Rivr.Models.Callbacks;

/// <summary>
/// Represents the order cancelled.
/// </summary>
public class OrderCancelled
{
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the cancelled date.
    /// </summary>
    public DateTime CancelledDate { get; set; }

    /// <summary>
    /// Gets or sets the cancellation reason.
    /// </summary>
    public CancellationReason Reason { get; set; }
}