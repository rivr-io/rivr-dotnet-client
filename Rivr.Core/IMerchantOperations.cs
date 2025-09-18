using System;
using System.Threading.Tasks;
using Rivr.Core.Models;
using Rivr.Core.Models.Devices;
using Rivr.Core.Models.Orders;
using Rivr.Core.Models.OrderSettlements;
using Rivr.Core.Models.Subscriptions;
using Rivr.Core.Models.Webhooks;

namespace Rivr.Core;

/// <summary>
/// Represents the operations that can be performed on behalf of a merchant.
/// </summary>
public interface IMerchantOperations
{
    /// <summary>
    /// Gets the health of the Rivr API.
    /// </summary>
    /// <returns></returns>
    Task<Health> GetHealthSecureAsync();

    /// <summary>
    /// Gets the merchant's devices.
    /// </summary>
    /// <returns></returns>
    Task<Device[]> GetDevicesAsync();

    /// <summary>
    /// Creates an order.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    Task<Order> CreateOrderAsync(CreateOrderRequest order);

    /// <summary>
    /// Gets an order.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    Task<Order> GetOrderAsync(Guid orderId);

    /// <summary>
    /// Refunds or cancels an order depending on the current status.
    /// From the status <see cref="OrderStatus.Completed"/>, the order will be refunded.
    /// From the status <see cref="OrderStatus.Created"/>, the order will be cancelled.
    /// The calling system does not need to know the current status of the order.
    /// Note: There are certain payment methods that do not support refunds (some instalment products).
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    Task RefundAsync(Guid orderId);

    /// <summary>
    /// Get a list of order settlements
    /// </summary>
    Task<OrderSettlementForLists[]> GetOrderSettlementsAsync();

    /// <summary>
    /// Get last unread order settlement
    /// </summary>
    Task<OrderSettlement> GetLastUnreadOrderSettlementAsync();

    /// <summary>
    /// Get the next unread order settlement as a Nets file.
    /// </summary>
    /// <returns>
    /// The content of the file as a string.
    /// If there are no more unread order settlements, <see langword="null"/> is returned.
    /// </returns>
    Task<string> GetNextUnreadOrderSettlementAsNetsFile();

    /// <summary>
    /// Creates or updates a subscription for the merchant.
    /// </summary>
    /// <param name="createSubscriptionRequest"></param>
    /// <returns></returns>
    Task CreateOrUpdateSubscriptionAsync(CreateSubscriptionRequest createSubscriptionRequest);

    /// <summary>
    /// Operations for fetching webhooks via the webhook aggregator.
    /// </summary>
    IWebhookAggregatorOperations Webhooks { get; }
}

/// <summary>
/// Represents the operations that can be performed on the webhook aggregator.
/// </summary>
public interface IWebhookAggregatorOperations
{
    /// <summary>
    /// Creates a webhook aggregation bundle for the merchant.
    /// </summary>
    /// <returns></returns>
    Task CreateBundleAsync();

    /// <summary>
    /// Gets the bundles available for the merchant.
    /// </summary>
    /// <returns></returns>
    Task<Bundle> GetNextUnreadBundleAsync();

    /// <summary>
    /// Gets the webhooks in a specific bundle.
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns></returns>
    Task<Bundle> GetBundleAsync(Guid bundleId);

    /// <summary>
    /// Marks a bundle as read.
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns></returns>
    Task MarkBundleAsReadAsync(Guid bundleId);
}