using System;
using System.Linq;

namespace Rivr.Core.Models.OrderSettlements;

/// <summary>
/// Order settlement.
/// </summary>
public class OrderSettlement
{
    /// <summary>
    /// The order settlement ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The date the order settlement was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// The merchant ID.
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// The merchant name.
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// The date the order settlement was prepared.
    /// </summary>
    public DateTime? PreparedDate { get; set; }

    /// <summary>
    /// The date the payout was requested.
    /// </summary>
    public DateTime? PayoutRequestedDate { get; set; }

    /// <summary>
    /// The date the payout was completed.
    /// </summary>
    public DateTime? PayoutCompletedDate { get; set; }

    /// <summary>
    /// The date the settlement was read through account balancing.
    /// </summary>
    public DateTime? ReadThroughAccountBalancingDate { get; set; }

    /// <summary>
    /// The reference.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// The settlement number.
    /// </summary>
    public string? SettlementNumber { get; set; }

    /// <summary>
    /// The orders by payment method.
    /// </summary>
    public OrdersByPaymentMethod[] OrdersByPaymentMethod { get; set; } = [];

    /// <summary>
    /// The OCR number.
    /// </summary>
    public string? OCR;

    /// <summary>
    /// Total amount of the settlement 
    /// </summary>
    public decimal TotalAmount => OrdersByPaymentMethod.Sum(o => o.TotalAmount);

    /// <summary>
    /// The number of completed orders.
    /// </summary>
    public int CompletedCount => OrdersByPaymentMethod.Sum(opm => opm.CompletedCount);

    /// <summary>
    /// The number of refunded orders. 
    /// </summary>
    public int RefundedCount => OrdersByPaymentMethod.Sum(opm => opm.RefundedCount);

    /// <summary>
    /// The total amount of completed orders.
    /// </summary>
    public decimal CompletedAmount => OrdersByPaymentMethod.Sum(opm => opm.CompletedAmount);

    /// <summary>
    /// The total amount of refunded orders.
    /// </summary>
    public decimal RefundedAmount => OrdersByPaymentMethod.Sum(opm => opm.RefundedAmount);

    /// <summary>
    /// If the Order Settlement is a credit.
    /// </summary>
    public bool IsCredit { get; set; }
}
