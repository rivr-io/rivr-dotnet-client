namespace Rivr.Core.Models.Orders;

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
    /// The first name of the customer.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The last name of the customer.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The phone number of the customer.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The street address of the customer.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The city of the customer.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// The postal/zip code of the customer.
    /// </summary>
    public string? PostalCode { get; set; }
}
