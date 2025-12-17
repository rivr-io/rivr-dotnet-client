namespace Rivr.Core.Models;

/// <summary>
/// Represents and error that occurred during an API call.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// The name of the property that was causing errors if any.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Message stating what was invalid with the property
    /// </summary>
    public string? Message { get; set; }
}
