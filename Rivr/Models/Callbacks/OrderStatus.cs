namespace Rivr.Models.Callbacks;

/// <summary>
/// Represents the order status.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order was created.
    /// </summary>
    Created,

    /// <summary>
    /// The order was completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The order was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The order was refunded.
    /// </summary>
    Refunded
}