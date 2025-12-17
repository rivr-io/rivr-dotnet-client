namespace Rivr.Core.Models;

/// <summary>
/// Validation errors 
/// </summary>
public class OrderRequestError
{
    /// <summary>
    /// The name of the property that was causing errors.
    /// </summary>
    /// <example>"Amount"</example>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Message stating what was invalid with the property
    /// </summary>
    /// <example>"Amount cannot be zero or negative"</example>
    public string? Message { get; set; }
}
