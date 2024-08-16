using System;

namespace Rivr.Models.Orders;

public class Order
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string? ShortId { get; set; }
    public string? MerchantName { get; set; }
    public Customer Customer { get; set; } = new();
    public OrderLine[] OrderLines { get; set; } = [];
    public decimal Amount { get; set; }
    public decimal AmountExclVat { get; set; }
    public decimal VatAmount { get; set; }
    public string? Reference { get; set; }
    public string? OCR { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public bool Success { get; set; }
    public string? CallbackUrl { get; set; }
    public CheckoutHint[] CheckoutHints { get; set; } = [];
}