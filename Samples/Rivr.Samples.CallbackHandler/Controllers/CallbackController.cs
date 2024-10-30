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
        [HttpPost]
        public Task Post([FromBody] Callback callback)
        {
            logger.LogInformation("Received callback: {Callback}", callback);

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
                    logger.LogInformation("Order created: {orderCreated}", orderCreated);
                    break;
                case OrderStatus.Pending:
                    var orderPending = callback.Data.Deserialise<OrderPending>();
                    logger.LogInformation("Order pending: {orderPending}", orderPending);
                    break;
                case OrderStatus.Completed:
                    var orderCompleted = callback.Data.Deserialise<OrderCompleted>();
                    logger.LogInformation("Order completed: {orderCompleted}", orderCompleted);
                    break;
                case OrderStatus.Cancelled:
                    var orderCancelled = callback.Data.Deserialise<OrderCancelled>();
                    logger.LogInformation("Order cancelled: {orderCancelled}", orderCancelled);
                    break;
                case OrderStatus.Refunded:
                    var orderRefunded = callback.Data.Deserialise<OrderRefunded>();
                    logger.LogInformation("Order refunded: {orderRefunded}", orderRefunded);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}