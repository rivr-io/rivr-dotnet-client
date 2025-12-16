using System;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// Information about an order receipt.
/// </summary>
public class OrderReceiptInfo
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
    /// The format of the receipt.
    /// </summary>
    public ReceiptFormat ReceiptFormat { get; set; }
}
