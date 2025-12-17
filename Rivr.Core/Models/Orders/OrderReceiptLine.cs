namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents a line on an order receipt.
/// </summary>
public class OrderReceiptLine
{
    /// <summary>
    /// The key of the receipt line.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// The value of the receipt line.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// The name of the receipt line.
    /// </summary>
    public string? Name { get; set; }
}
