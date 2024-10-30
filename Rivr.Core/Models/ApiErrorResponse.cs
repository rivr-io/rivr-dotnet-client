namespace Rivr.Core.Models;

/// <summary>
/// Represents and error that occurred during an API call.
/// </summary>
/// <param name="propertyName"></param>
/// <param name="message"></param>
public class ApiErrorResponse(string propertyName, string message)
{
    /// <summary>
    /// The name of the property that was causing errors if any.
    /// </summary>
    public string PropertyName { get; set; } = propertyName;

    /// <summary>
    /// Message stating what was invalid with the property
    /// </summary>
    public string Message { get; set; } = message;
}