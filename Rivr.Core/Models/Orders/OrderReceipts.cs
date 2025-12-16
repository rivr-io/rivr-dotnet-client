using System;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents the full receipt details with all receipt lines.
/// </summary>
public class OrderReceipts
{
    /// <summary>
    /// The ID of the receipt.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The date of the receipt.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The payment method of the receipt.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// The customer receipt lines.
    /// </summary>
    public OrderReceiptLine[]? CustomerReceipt { get; set; }

    /// <summary>
    /// The cashier receipt lines.
    /// </summary>
    public OrderReceiptLine[]? CashierReceipt { get; set; }

    /// <summary>
    /// The refund receipt lines.
    /// </summary>
    public OrderReceiptLine[]? RefundReceipt { get; set; }
}
