using System.Net;

namespace Rivr.Core.Models;

/// <summary>
/// Thrown by <see cref="Rivr.Core.IMerchantOperations.SendOrderToDeviceAsync"/> when the order was not sent to the device.
/// The message says why, for example that the order is not in status Created or that the device is not one of the
/// merchant's devices. Use <see cref="IsRetryable"/> to know whether the same call can succeed later.
/// </summary>
public class SendOrderToDeviceException : ApiCallException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SendOrderToDeviceException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="error">The error returned by the API. Its fields are empty when the response had no body.</param>
    public SendOrderToDeviceException(HttpStatusCode statusCode, ApiErrorResponse error)
        : base(error.Message ?? $"The order was not sent to the device ({(int)statusCode} {statusCode}).")
    {
        StatusCode = statusCode;
        PropertyName = error.PropertyName;
    }

    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The name of the property the API reported, if any: <c>DeviceId</c> for an unknown, offline or foreign device,
    /// <c>Status</c> for an order that is not in status Created, <c>orderId</c> for an order that was not found.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// <see langword="true"/> when the device is offline (HTTP 409). The same call succeeds once the device is online
    /// again; see <see cref="Devices.Device.IsOnline"/>.
    /// </summary>
    public bool IsRetryable => StatusCode == HttpStatusCode.Conflict;
}
