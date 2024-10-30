using System;

namespace Rivr.Core.Models.OrderSettlements;

/// <summary>
/// Represents an order settlement for lists.
/// </summary>
public class OrderSettlementForLists
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
    public string MerchantName { get; set; }

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
    public string Reference { get; set; }

    /// <summary>
    /// The settlement number.
    /// </summary>
    public string SettlementNumber { get; set; }

    /// <summary>
    /// The total amount of the order settlement.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The total number of completed orders in the settlement.
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// The total number of refunded orders in the settlement.
    /// </summary>
    public int RefundedCount { get; set; }

    /// <summary>
    /// The total amount of completed orders in the settlement.
    /// </summary>
    public decimal CompletedAmount { get; set; }

    /// <summary>
    /// The total amount of refunded orders in the settlement.
    /// </summary>
    public decimal RefundedAmount { get; set; }
}