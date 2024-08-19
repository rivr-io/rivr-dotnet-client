namespace Rivr.Models.Orders;

/// <summary>
/// Represents a customer.
/// </summary>
public class Customer
{
    /// <summary>
    /// The personal number of the customer.
    /// </summary>
    public string? PersonalNumber { get; set; }

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The phone number of the customer.
    /// </summary>
    public string? Phone { get; set; }
}