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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health status.</returns>
    Task<Health> GetHealthSecureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the merchant's devices.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An array of devices.</returns>
    Task<Device[]> GetDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an order.
    /// </summary>
    /// <param name="order">The order to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created order.</returns>
    Task<Order> CreateOrderAsync(CreateOrderRequest order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order.</returns>
    Task<Order> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

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
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefundAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of order settlements.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An array of order settlements.</returns>
    Task<OrderSettlementForLists[]> GetOrderSettlementsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get last unread order settlement.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The last unread order settlement.</returns>
    Task<OrderSettlement> GetLastUnreadOrderSettlementAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the next unread order settlement as a Nets file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The content of the file as a string.
    /// If there are no more unread order settlements, <see langword="null"/> is returned.
    /// </returns>
    Task<string?> GetNextUnreadOrderSettlementAsNetsFile(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a subscription for the merchant.
    /// </summary>
    /// <param name="createSubscriptionRequest">The subscription request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateOrUpdateSubscriptionAsync(CreateSubscriptionRequest createSubscriptionRequest, CancellationToken cancellationToken = default);

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