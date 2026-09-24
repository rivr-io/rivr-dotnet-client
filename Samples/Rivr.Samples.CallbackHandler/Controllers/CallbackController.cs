using Microsoft.AspNetCore.Mvc;
using Rivr.Core.Models.Callbacks;
using Rivr.Core.Models.Orders;
using Rivr.Extensions;

namespace Rivr.Samples.CallbackHandler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CallbackController(ILogger<CallbackController> logger) : ControllerBase
    {
        // Log identifiers and statuses only, never the callback or its data. The data can contain personal
        // data (customer details, metadata you attached to the order), and log files are kept, copied and
        // shared far more widely than your database.

        [HttpPost]
        public Task Post([FromBody] Callback callback)
        {
            // Only typed values: Status is a free-form string from the request, and logging it as-is would let a
            // caller write arbitrary text (including line breaks) into your log. It is logged below once parsed.
            logger.LogInformation(
                "Received {CallbackType} callback {CallbackId} for merchant {MerchantId}",
                callback.Type, callback.Id, callback.MerchantId);

            switch (callback.Type)
            {
                case CallbackType.Order:
                    HandleOrderCallback(callback);
                    break;

                default:
                    logger.LogWarning("Unhandled callback type: {CallbackType}", callback.Type);
                    break;
            }

            return Task.CompletedTask;
        }

        private void HandleOrderCallback(Callback callback)
        {
            var orderStatus = Enum.Parse<OrderStatus>(callback.Status ?? string.Empty);
            switch (orderStatus)
            {
                case OrderStatus.Created:
                    var orderCreated = callback.Data.Deserialise<OrderCreated>();
                    logger.LogInformation("Order {OrderId} created at {CreatedDate}", callback.Id, orderCreated.CreatedDate);
                    break;
                case OrderStatus.Pending:
                    var orderPending = callback.Data.Deserialise<OrderPending>();
                    logger.LogInformation("Order {OrderId} pending since {CreatedDate}", callback.Id, orderPending.CreatedDate);
                    break;
                case OrderStatus.Completed:
                    var orderCompleted = callback.Data.Deserialise<OrderCompleted>();
                    logger.LogInformation("Order {OrderId} completed at {CompletedDate} with {PaymentMethod}",
                        callback.Id, orderCompleted.CompletedDate, orderCompleted.PaymentMethod);
                    break;
                case OrderStatus.Cancelled:
                    var orderCancelled = callback.Data.Deserialise<OrderCancelled>();
                    logger.LogInformation("Order {OrderId} cancelled at {CancelledDate} ({Reason})",
                        callback.Id, orderCancelled.CancelledDate, orderCancelled.Reason);
                    break;
                case OrderStatus.Refunded:
                    var orderRefunded = callback.Data.Deserialise<OrderRefunded>();
                    logger.LogInformation("Order {OrderId} refunded at {RefundedDate}", callback.Id, orderRefunded.RefundedDate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
