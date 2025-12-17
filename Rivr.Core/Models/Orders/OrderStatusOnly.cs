using System;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// Lightweight order representation containing only ID and status.
/// </summary>
public class OrderStatusOnly
{
    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }
}
