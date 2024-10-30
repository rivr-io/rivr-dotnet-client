﻿using Rivr.Models.Merchants;

namespace Rivr.Models.OrderSettlements;

/// <summary>
/// Represents the response from the GetOrderSettlements operation.
/// </summary>
public class GetOrderSettlementsResponse
{
    /// <summary>
    /// The order settlements.
    /// </summary>
    public OrderSettlementForLists[] OrderSettlements { get; set; } = [];

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public Metadata Metadata { get; set; } = new();
}