using System.Net;
using System.Net.Http;

namespace Rivr.Core.Models;

/// <summary>
/// Thrown when the Rivr API responds with a status code that does not indicate success and no more specific
/// exception applies (<see cref="UnauthorizedException"/>, <see cref="ForbiddenException"/>,
/// <see cref="CancelOrderException"/>). It is an <see cref="HttpRequestException"/>, so existing
/// <c>catch (HttpRequestException)</c> blocks keep working.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="System.Exception.Message"/> carries the status code, the HTTP method, the host, the request path
/// without its query string and, when available, a correlation id. It never carries the request or response
/// body: those can contain personal data (for example a personal identity number), and exception messages end up
/// in log files and error trackers.
/// </para>
/// <para>
/// When you need the API's error details, read <see cref="ResponseContent"/>. Do not log it.
/// </para>
/// </remarks>
public class RivrHttpRequestException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RivrHttpRequestException"/> class.
    /// </summary>
    /// <param name="message">A message without request or response bodies.</param>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="reasonPhrase">The reason phrase returned by the API.</param>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="requestPath">The request path without query string, with values that could identify a person masked.</param>
    /// <param name="correlationId">The correlation id of the request, if one was available.</param>
    /// <param name="responseContent">The response body. May contain personal data.</param>
    public RivrHttpRequestException(
        string message,
        HttpStatusCode statusCode,
        string? reasonPhrase,
        string? method,
        string? requestPath,
        string? correlationId,
        string? responseContent)
        : this(message, statusCode, reasonPhrase, method, requestPath, correlationId, responseContent, code: null, type: null, requestId: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RivrHttpRequestException"/> class.
    /// </summary>
    /// <param name="message">A message without request or response bodies.</param>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="reasonPhrase">The reason phrase returned by the API.</param>
    /// <param name="method">The HTTP method of the request.</param>
    /// <param name="requestPath">The request path without query string, with values that could identify a person masked.</param>
    /// <param name="correlationId">The correlation id of the request, if one was available.</param>
    /// <param name="responseContent">The response body. May contain personal data.</param>
    /// <param name="code">The API's stable error code, if the response body carried one.</param>
    /// <param name="type">The address of the error's page, if the response body carried one.</param>
    /// <param name="requestId">The <c>Rivr-Request-Id</c> header of the response, if it carried one.</param>
    public RivrHttpRequestException(
        string message,
        HttpStatusCode statusCode,
        string? reasonPhrase,
        string? method,
        string? requestPath,
        string? correlationId,
        string? responseContent,
        string? code,
        string? type,
        string? requestId)
        : base(message)
    {
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
        Method = method;
        RequestPath = requestPath;
        CorrelationId = correlationId;
        ResponseContent = responseContent;
        Code = code;
        Type = type;
        RequestId = requestId;
    }

    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    /// <remarks>
    /// The library targets .NET Standard 2.0, where <see cref="HttpRequestException"/> has no status code. On
    /// .NET 5 and later the base class's own <c>StatusCode</c> is therefore not set; use this property.
    /// </remarks>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The reason phrase returned by the API.
    /// </summary>
    public string? ReasonPhrase { get; }

    /// <summary>
    /// The HTTP method of the request, for example <c>POST</c>.
    /// </summary>
    public string? Method { get; }

    /// <summary>
    /// The request path without query string. Segments that could carry a value identifying a person are
    /// replaced by <c>{value}</c>; identifiers such as order and merchant ids are kept.
    /// </summary>
    public string? RequestPath { get; }

    /// <summary>
    /// The correlation id (<c>X-Correlation-Id</c>) of the request, when the API or the request carried a
    /// well-formed one. Include it when you contact Rivr support.
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// The response body returned by the API. <b>May contain personal data: do not log it.</b> It is not part of
    /// <see cref="System.Exception.Message"/> or <see cref="System.Exception.ToString"/>.
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// The API's stable error code, for example <c>order_not_found</c>, read from the response body, or
    /// <see langword="null"/> when the body had none (or was not a JSON object). Every error answer under
    /// <c>/api/public</c> has carried a code since 2026-10-25. Decide what to do from this, not from
    /// <see cref="System.Exception.Message"/> and not by parsing <see cref="ResponseContent"/> yourself: the message and
    /// the text of the body are for people and can change.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// The address of the page that explains the error, <c>https://developers.rivr.io/errors/{code}</c>, read from the
    /// response body, or <see langword="null"/> when the body had none.
    /// </summary>
    public string? Type { get; }

    /// <summary>
    /// The <c>Rivr-Request-Id</c> header of the response, a GUID that identifies the call at Rivr, or
    /// <see langword="null"/> when the response had none. Quote it when you contact Rivr support. It is not
    /// <see cref="CorrelationId"/>, which is the <c>X-Correlation-Id</c> of the request.
    /// </summary>
    public string? RequestId { get; }
}
