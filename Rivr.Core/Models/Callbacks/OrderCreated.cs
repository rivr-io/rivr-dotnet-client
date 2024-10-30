using System;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents the order created.
/// </summary>
public class OrderCreated
{
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}