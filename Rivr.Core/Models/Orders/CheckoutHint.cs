namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents a hint for the checkout.
/// </summary>
public class CheckoutHint
{
    /// <summary>
    /// The type of the checkout hint.
    /// </summary>
    public CheckoutHintType Type { get; set; }

    /// <summary>
    /// The value of the checkout hint.
    /// </summary>
    public string Value { get; set; }
}