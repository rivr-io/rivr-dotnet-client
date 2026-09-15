namespace Rivr.Core.Models.Orders;

/// <summary>
/// The error codes available on <see cref="CancelOrderException.ErrorCode"/> when
/// <see cref="Rivr.Core.IMerchantOperations.CancelAsync"/> does not cancel an order.
/// HTTP 409 means the same call can succeed later (<see cref="CancelOrderException.IsRetryable"/>);
/// HTTP 400 means retrying will not change the answer.
/// The codes say what to do, not which payment method is involved — the message carries the details.
/// </summary>
public static class CancelOrderErrorCodes
{
    /// <summary>
    /// 409 — a payment is in progress right now, for example a card payment on a terminal.
    /// Retry once it has completed or failed.
    /// </summary>
    public const string PaymentInProgress = "payment_in_progress";

    /// <summary>
    /// 409 — the order changed while it was being cancelled and was not cancelled.
    /// Retry to get the order's current state.
    /// </summary>
    public const string OrderChanged = "order_changed";

    /// <summary>
    /// 400 — the order has been paid. Use <see cref="Rivr.Core.IMerchantOperations.RefundAsync"/> to refund it.
    /// </summary>
    public const string OrderPaid = "order_paid";

    /// <summary>
    /// 400 — a payment for the order has been started and can no longer be stopped, for example an invoice
    /// that has not been paid yet. Follow the order status.
    /// </summary>
    public const string PaymentPending = "payment_pending";

    /// <summary>
    /// 400 — the order has already been refunded.
    /// </summary>
    public const string OrderRefunded = "order_refunded";

    /// <summary>
    /// 400 — the order is in a status that cannot be cancelled.
    /// </summary>
    public const string OrderNotCancellable = "order_not_cancellable";

    /// <summary>
    /// 404 — there is no order with the given id.
    /// </summary>
    public const string OrderNotFound = "order_not_found";
}
