namespace Rivr.Models.Orders;

/// <summary>
/// Represents the cancellation reason.
/// </summary>
public enum CancellationReason
{
    /// <summary>
    /// Tho order was cancelled due to a timeout.
    /// </summary>
    Timeout = 0,

    /// <summary>
    /// The user cancelled the order.
    /// </summary>
    UserCancelled = 1 << 0,

    /// <summary>
    /// The merchant cancelled the order.
    /// </summary>
    MerchantCancelled = 1 << 1,
}