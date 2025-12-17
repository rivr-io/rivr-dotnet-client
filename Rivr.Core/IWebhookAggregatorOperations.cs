using System;
using System.Threading.Tasks;
using Rivr.Core.Models.Webhooks;

namespace Rivr.Core;

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
    /// <returns>The next unread bundle, or null if there are no unread bundles.</returns>
    Task<Bundle?> GetNextUnreadBundleAsync();

    /// <summary>
    /// Gets the webhooks in a specific bundle.
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns>The bundle, or null if not found.</returns>
    Task<Bundle?> GetBundleAsync(Guid bundleId);

    /// <summary>
    /// Marks a bundle as read.
    /// </summary>
    /// <param name="bundleId"></param>
    /// <returns></returns>
    Task MarkBundleAsReadAsync(Guid bundleId);
}
