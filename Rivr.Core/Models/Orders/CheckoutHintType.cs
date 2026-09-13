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

    /// <summary>
    /// Represents an auto cancel checkout hint in seconds.
    /// </summary>
    AutoCancelInSeconds = 1 << 4,

    // 1 << 5 is ChargeStoredCard in the Rivr contracts. It is not exposed here yet; the value is
    // reserved so the numbering stays aligned with the platform.

    /// <summary>
    /// Leave the order line descriptions in PLAIN TEXT for this order instead of encrypting them
    /// as the merchant's sensitive-data setting would otherwise require.
    /// </summary>
    /// <remarks>
    /// Without this hint a generic label ("Behandling / Åtgärd") is shown until the patient
    /// unlocks the line — correct for a treatment, wrong for a line that is not one: an advance
    /// payment, a fee, an account deposit. Set it per order, and only for text you wrote
    /// yourself: whatever you put in the description is what the patient sees on the receipt and
    /// what clinic staff see in the portal.
    /// <para>
    /// Requires <c>Value = "true"</c>. Any other value — including none — leaves the line
    /// encrypted, the same rule <see cref="DisableNotifications"/> follows.
    /// </para>
    /// </remarks>
    DisableDescriptionEncryption = 1 << 6,
}