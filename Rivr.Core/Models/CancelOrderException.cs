using System.Net;

namespace Rivr.Core.Models;

/// <summary>
/// Thrown by <see cref="Rivr.Core.IMerchantOperations.CancelAsync"/> when the order was not cancelled.
/// Nothing was cancelled and nothing was refunded.
/// Use <see cref="ErrorCode"/> (see <see cref="Orders.CancelOrderErrorCodes"/>) to decide what to do,
/// and <see cref="IsRetryable"/> to know whether the same call can succeed later.
/// </summary>
public class CancelOrderException : ApiCallException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelOrderException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="error">The error returned by the API. Its fields are empty when the response had no body.</param>
    public CancelOrderException(HttpStatusCode statusCode, ApiErrorResponse error)
        : base(error.Message ?? $"The order was not cancelled ({(int)statusCode} {statusCode}).")
    {
        StatusCode = statusCode;
        ErrorCode = error.ErrorCode;
        PropertyName = error.PropertyName;
    }

    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Why the order was not cancelled. One of <see cref="Orders.CancelOrderErrorCodes"/>, or
    /// <see langword="null"/> when the API did not return a code.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// The name of the property the API reported, if any.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// <see langword="true"/> when the same call can succeed later (HTTP 409), for example once a payment in
    /// progress has completed or failed.
    /// </summary>
    public bool IsRetryable => StatusCode == HttpStatusCode.Conflict;
}
