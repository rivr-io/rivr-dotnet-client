using System;
using System.Text.Json.Nodes;

namespace Rivr.Core.Models.Webhooks;

/// <summary>
/// Webhook bundle information.
/// </summary>
public class Bundle
{
    /// <summary>
    /// The bundle ID.
    /// </summary>
    public Guid BundleId { get; set; }

    /// <summary>
    /// Array of webhooks in this bundle.
    /// </summary>
    public Webhook[] Webhooks { get; set; } = [];
}

/// <summary>
/// Represents a single webhook notification.
/// </summary>
public class Webhook
{
    /// <summary>
    /// The ID of the entity this webhook relates to.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// The type of entity (e.g., "Order").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// The ID of the merchant associated with this webhook.
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// The status or event type of the webhook.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Additional data payload for the webhook.
    /// </summary>
    public JsonNode? Data { get; set; }

    /// <summary>
    /// The ID of the bundle this webhook belongs to.
    /// </summary>
    public Guid? BundleId { get; set; }

    /// <summary>
    /// The date and time when this webhook was read.
    /// </summary>
    public DateTime? ReadDate { get; set; }
}

/// <summary>
/// Response when retrieving a specific bundle.
/// </summary>
public class GetBundleResponse
{
    /// <summary>
    /// The unique identifier of the bundle.
    /// </summary>
    public Guid BundleId { get; set; }

    /// <summary>
    /// Array of webhooks in this bundle.
    /// </summary>
    public Webhook[] Webhooks { get; set; } = [];
}
