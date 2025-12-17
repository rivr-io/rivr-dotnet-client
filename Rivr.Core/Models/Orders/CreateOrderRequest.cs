using System;
using System.Collections.Generic;
using System.Linq;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// The Payment Request Object is used in all payment request operations and the provided data object (body) should be in JSON format. 
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Payment request ID (UUID). This is the same ID used as Path parameter named <code>paymentRequestId</code>/>
    /// </summary>
    /// <example>497f6eca-6276-4993-bfeb-53cbbbba6f08</example>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique identifier (UUID) of the Merchant (available in the Merchant portal)
    /// </summary>
    /// <example>c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b</example>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// The order lines that the payment request is for.
    /// </summary>
    public OrderLine[] OrderLines { get; set; } = [];

    /// <summary>
    /// A unique payment reference. This should be your internally unique payment reference (ie an order number or other reference).
    /// </summary>
    /// <example>order-1234</example>
    public string? Reference { get; set; }

    /// <summary>
    /// Computed readonly amount of amount to pay.
    /// </summary>
    /// <example>42000</example>
    public int Amount => (int)OrderLines.Sum(o => o.Quantity * o.UnitPriceExclVat * (1 + o.VatPercentage / 100.0m));

    /// <summary>
    /// The Payer of the payment. This is the person that will pay for the payment request.
    /// </summary>
    public string? PersonalNumber { get; set; }

    /// <summary>
    /// Email address of the Payer. This is the person that will pay for the payment request.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number of the Payer. This is the person that will pay for the payment request.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The checkout hints that the payment request is for.
    /// </summary>
    public CheckoutHint[] CheckoutHints { get; set; } = [];

    /// <summary>
    /// URL that Rivr will use to notify caller about the outcome of the <see cref="CreateOrderRequest"/>. The URL has to use HTTPS.
    /// </summary>
    /// <example>https://www.example.com/my/callback</example>
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// A dictionary of key-value pairs that can be used to store additional information about the payment request.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
