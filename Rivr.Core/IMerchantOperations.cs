using System;
using System.Threading;
using System.Threading.Tasks;
using Rivr.Core.Models;
using Rivr.Core.Models.Devices;
using Rivr.Core.Models.Heartbeats;
using Rivr.Core.Models.Merchants;
using Rivr.Core.Models.Orders;
using Rivr.Core.Models.OrderSettlements;
using Rivr.Core.Models.SatelliteServices;
using Rivr.Core.Models.Subscriptions;

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
    /// Gets the status of an order (lightweight compared to GetOrderAsync).
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderStatusOnly> GetOrderStatusAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific receipt for an order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <param name="receiptId">The ID of the receipt.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderReceipts> GetOrderReceiptAsync(Guid orderId, Guid receiptId, CancellationToken cancellationToken = default);

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
    /// Sends a heartbeat.
    /// </summary>
    /// <param name="heartbeat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HeartbeatResponse> SendHeartbeatAsync(SendHeartbeatRequest heartbeat, CancellationToken cancellationToken = default);

    /// <summary>
    /// Operations for fetching webhooks via the webhook aggregator.
    /// </summary>
    IWebhookAggregatorOperations Webhooks { get; }

    /// <summary>
    /// Gets the merchant details.
    /// </summary>
    /// <param name="merchantId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Merchant> GetMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets virtual terminals for a satellite service.
    /// </summary>
    /// <param name="uniqueServiceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<VirtualTerminal[]> GetVirtualTerminalsAsync(string uniqueServiceId, CancellationToken cancellationToken = default);
}