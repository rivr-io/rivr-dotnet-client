using System;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents a line in an order.
/// </summary>
public class OrderLine
{
    private int _vatPercentage;

    /// <summary>
    /// The name of the product or service
    /// </summary>
    /// <example>Product 1</example>
    public string Description { get; set; }

    /// <summary>
    /// The quantity of the product or service
    /// </summary>
    /// <example>1</example>
    public decimal Quantity { get; set; }

    /// <summary>
    /// The price of the product or service including VAT
    /// </summary>
    /// <example>2500</example>
    public decimal UnitPriceExclVat { get; set; }

    /// <summary>
    /// The VAT rate of the product or service (0, 6, 12 or 25)
    /// </summary>
    /// <example>25</example>
    public int VatPercentage
    {
        get => _vatPercentage;
        set
        {
            const int minVatAmount = 0;
            const int maxVatAmount = 25;
            if (value is < minVatAmount or > maxVatAmount)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Vat percentage must be between {minVatAmount} and {maxVatAmount}");
            }

            _vatPercentage = value;
        }
    }

    /// <summary>
    /// The price of the product or service excluding VAT
    /// </summary>
    public decimal UnitPriceInclVat => UnitPriceExclVat * (1 + VatPercentage / 100m);

    /// <summary>
    /// The total price of the product or service including VAT
    /// </summary>
    public decimal AmountInclVat => UnitPriceInclVat * Quantity;

    /// <summary>
    /// The total price of the product or service excluding VAT
    /// </summary>
    public decimal AmountExclVat => AmountInclVat / (1 + VatPercentage);

    private decimal VatAmount => AmountInclVat - AmountExclVat;
}