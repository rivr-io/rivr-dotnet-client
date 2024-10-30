namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents the type of checkout hint.
/// </summary>
public enum CheckoutHintType
{
    /// <summary>
    /// Represents a device checkout hint.
    /// </summary>
    Device = 1 << 0
}