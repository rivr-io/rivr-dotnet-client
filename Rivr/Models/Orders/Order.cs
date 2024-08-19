using System;

namespace Rivr.Models.Orders;

/// <summary>
/// Represents an order.
/// </summary>
public class Order
{
    /// <summary>
    /// The ID of the order.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the merchant to which the order belongs.
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// The name of the merchant to which the order belongs.
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// The short ID of the order.
    /// </summary>
    public string? ShortId { get; set; }

    /// <summary>
    /// The customer of the order.
    /// </summary>
    public Customer Customer { get; set; } = new();

    /// <summary>
    /// The order lines of the order.
    /// </summary>
    public OrderLine[] OrderLines { get; set; } = [];

    /// <summary>
    /// The total amount of the order.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The amount of the order excluding VAT.
    /// </summary>
    public decimal AmountExclVat { get; set; }

    /// <summary>
    /// The VAT amount of the order.
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// The order reference.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// The payment method of the order.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// The status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// The callback URL of the order.
    /// </summary>
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// The checkout hints of the order.
    /// </summary>
    public CheckoutHint[] CheckoutHints { get; set; } = [];
}