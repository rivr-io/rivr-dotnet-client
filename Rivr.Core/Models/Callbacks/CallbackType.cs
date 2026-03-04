using System.Text.Json.Serialization;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents the entity type of the callback.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CallbackType
{
    /// <summary>
    /// Represents an order.
    /// </summary>
    Order,

    /// <summary>
    /// Represents an invoice.
    /// </summary>
    Invoice
}