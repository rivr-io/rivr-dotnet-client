namespace Rivr.Models.Orders;

/// <summary>
/// Represents the status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// No status (unexpected).
    /// </summary>
    None = 0,

    /// <summary>
    /// The order has been created.
    /// </summary>
    Created = 1 << 0,

    /// <summary>
    /// The order has been completed (paid or transferred to an invoice).
    /// </summary>
    Completed = 1 << 2,

    /// <summary>
    /// The order has been refunded.
    /// </summary>
    Refunded = 1 << 3,

    /// <summary>
    /// The order has been cancelled.
    /// </summary>
    Cancelled = 1 << 5,
}