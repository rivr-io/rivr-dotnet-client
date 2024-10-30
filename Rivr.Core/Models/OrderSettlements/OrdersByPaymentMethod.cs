using Rivr.Core.Models.Orders;

namespace Rivr.Core.Models.OrderSettlements;

/// <summary>
/// Represents an aggregated view of orders in an order settlement.
/// </summary>
public class OrdersByPaymentMethod
{
    /// <summary>
    /// The payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// The total amount of the orders.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The total amount of completed orders.
    /// </summary>
    public decimal CompletedAmount { get; set; }

    /// <summary>
    /// The total amount of refunded orders.
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// The total number of orders.
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// The total number of refunded orders.
    /// </summary>
    public int RefundedCount { get; set; }

    /// <summary>
    /// The completed orders in the settlement.
    /// </summary>
    public OrderSettlementOrder[] Orders { get; set; } = [];

    /// <summary>
    /// The refunded orders in the settlement.
    /// </summary>
    public OrderSettlementOrder[] RefundedOrders { get; set; } = [];
}