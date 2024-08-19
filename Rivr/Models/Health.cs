namespace Rivr.Models;

/// <summary>
/// Represents the health of the Rivr API.
/// </summary>
public class Health
{
    /// <summary>
    /// The title of the health response
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The status of the health response
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// The message of the health response
    /// </summary>
    public string? Message { get; set; }
}