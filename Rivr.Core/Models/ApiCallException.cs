using System;

namespace Rivr.Core.Models;

/// <summary>
/// Represents an error that occurred during an API call.
/// </summary>
public class ApiCallException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiCallException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public ApiCallException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiCallException"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ApiCallException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiCallException"/> class with what the API's error answer carried.
    /// </summary>
    /// <param name="message">A message without request or response bodies.</param>
    /// <param name="code">The API's stable error code, if the answer carried one.</param>
    /// <param name="type">The address of the error's page, if the answer carried one.</param>
    /// <param name="requestId">The <c>Rivr-Request-Id</c> header of the answer, if it carried one.</param>
    public ApiCallException(string message, string? code, string? type, string? requestId) : base(message)
    {
        Code = code;
        Type = type;
        RequestId = requestId;
    }

    /// <summary>
    /// The API's stable error code, for example <c>order_paid</c>, or <see langword="null"/> when the answer had none.
    /// Decide what to do from this, not from <see cref="Exception.Message"/>: the message is for people and can change.
    /// Every error answer under <c>/api/public</c> has carried a code since 2026-10-25.
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
