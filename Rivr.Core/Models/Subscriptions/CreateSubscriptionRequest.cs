using System;

namespace Rivr.Core.Models.Subscriptions;

/// <summary>
/// Create Subscription Request
/// </summary>
public class CreateSubscriptionRequest
{
    /// <summary>
    /// Personal Number
    /// </summary>
    public string? PersonalNumber { get; set; }

    /// <summary>
    /// Phone
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Merchant Id
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Subscription Payments
    /// </summary>
    public SubscriptionPayment[] SubscriptionPayments { get; set; } = [];
}

/// <summary>
/// Subscription Payment
/// </summary>
public class SubscriptionPayment
{
    /// <summary>
    /// Created Date
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Due Date
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// External Reference
    /// </summary>
    public string? ExternalReference { get; set; }
}
