using System;

namespace Rivr.Core.Models.Orders;

public class OrderStatusOnly
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
}

