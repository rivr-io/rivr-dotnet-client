using System.Text.Json.Serialization;

namespace Rivr.Core.Models.Orders;

/// <summary>
/// Represents the type of checkout hint.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CheckoutHintType
{
    /// <summary>
    /// Represents a device checkout hint.
    /// </summary>
    Device = 1 << 0,

    /// <summary>
    /// Represents a redirect checkout hint.
    /// </summary>
    Redirect = 1 << 1,

    /// <summary>
    /// Represents a disable notifications checkout hint.
    /// </summary>
    DisableNotifications = 1 << 2,

    /// <summary>
    /// Represents an advance payment checkout hint.
    /// </summary>
    AdvancePayment = 1 << 3,
}