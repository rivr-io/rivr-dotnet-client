namespace Rivr.Models.Orders;

public enum OrderStatus
{
    None = 0,
    Created = 1 << 0,
    Completed = 1 << 2,
    Refunded = 1 << 3,
    Cancelled = 1 << 5,
}