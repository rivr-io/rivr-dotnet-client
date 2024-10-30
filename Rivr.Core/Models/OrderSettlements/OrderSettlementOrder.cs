using System;
using System.Collections.Generic;
using Rivr.Core.Models.Orders;

namespace Rivr.Core.Models.OrderSettlements;

/// <summary>
/// Represents an order in an order settlement.
/// </summary>
public class OrderSettlementOrder
{
    /// <summary>
    /// The ID of the order.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// The order number.
    /// </summary>
    public string OrderNumber { get; set; }

    /// <summary>
    /// The date the order was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Order description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The total amount of the order including VAT.
    /// </summary>
    public decimal TotalAmountInclVat { get; set; }

    /// <summary>
    /// The total amount of the order excluding VAT.
    /// </summary>
    public decimal TotalAmountExclVat { get; set; }

    /// <summary>
    /// The VAT percentage of the order.
    /// </summary>
    public decimal VatPercentage { get; set; }

    /// <summary>
    /// The Payment Method of the order.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// The reference of the order.
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// The metadata of the order.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];
}