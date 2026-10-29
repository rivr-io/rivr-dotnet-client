using System;

namespace Rivr.Core.Models;

/// <summary>
/// This exception is thrown when the request is unauthorized.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <inheritdoc />
    public UnauthorizedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with what the API's error answer
    /// carried.
    /// </summary>
    /// <param name="message">A message without request or response bodies.</param>
    /// <param name="code">The API's stable error code, if the answer carried one.</param>
    /// <param name="type">The address of the error's page, if the answer carried one.</param>
    /// <param name="requestId">The <c>Rivr-Request-Id</c> header of the answer, if it carried one.</param>
    public UnauthorizedException(string message, string? code, string? type, string? requestId) : base(message)
    {
        Code = code;
        Type = type;
        RequestId = requestId;
    }

    /// <summary>
    /// The API's stable error code, for example <c>token_expired</c>, or <see langword="null"/> when the answer had
    /// none. The token endpoint answers with OAuth's <c>error</c> and no code.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// The address of the page that explains the error, <c>https://developers.rivr.io/errors/{code}</c>, or
    /// <see langword="null"/> when the answer had none.
    /// </summary>
    public string? Type { get; }

    /// <summary>
    /// The <c>Rivr-Request-Id</c> of the call, a GUID that identifies it at Rivr, or <see langword="null"/> when the
    /// answer had none. Quote it when you contact Rivr support.
    /// </summary>
    public string? RequestId { get; }
}
