using System;
using System.Text.Json;
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

    public Webhook[] Webhooks { get; set; } = [];
}

public class Webhook
{
    public Guid EntityId { get; set; }
    public string? EntityType { get; set; }
    public Guid MerchantId { get; set; }
    public string? Status { get; set; }
    public JsonNode Data { get; set; } = new JsonObject();
    public Guid? BundleId { get; set; }
    public DateTime? ReadDate { get; set; }
}

public class GetBundleResponse
{
    public Guid BundleId { get; set; }

    public Webhook[] Webhooks { get; set; } = [];
}