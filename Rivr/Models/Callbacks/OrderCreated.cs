﻿using System;
using Rivr.Models.Orders;

namespace Rivr.Models.Callbacks;

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