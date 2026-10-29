namespace Rivr.Core.Models;

/// <summary>
/// Represents an error response from the Rivr API.
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

    /// <summary>
    /// A machine-readable code identifying the error, if the API returned one.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// The API's stable error code, for example <c>order_paid</c>, if the response carried one. Every error answer
    /// under <c>/api/public</c> has carried it since 2026-10-25. The same value as <see cref="ErrorCode"/> where both
    /// are there; program against this one.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// The address of the page that explains the error, <c>https://developers.rivr.io/errors/{code}</c>, if the
    /// response carried one.
    /// </summary>
    public string? Type { get; set; }
}
