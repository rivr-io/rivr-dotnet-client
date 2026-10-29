using System;

namespace Rivr.Core.Models;

/// <summary>
/// This exception is thrown when the request is forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="error">The error code.</param>
    /// <param name="errorDescription">A human-readable description of the error.</param>
    public ForbiddenException(string error, string errorDescription) : base(errorDescription)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="errorResponse">The error response from the API.</param>
    public ForbiddenException(ErrorResponse errorResponse) : base(errorResponse.ErrorDescription)
    {
        Error = errorResponse.Error;
        ErrorDescription = errorResponse.ErrorDescription;
        Code = errorResponse.Code;
        Type = errorResponse.Type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="errorResponse">The error response from the API.</param>
    /// <param name="requestId">The <c>Rivr-Request-Id</c> header of the answer, if it carried one.</param>
    public ForbiddenException(ErrorResponse errorResponse, string? requestId) : this(errorResponse)
    {
        RequestId = requestId;
    }

    /// <summary>
    /// The error code.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// The API's stable error code, for example <c>invalid_scope</c>, or <see langword="null"/> when the answer
    /// had none. Decide what to do from this: <see cref="ErrorDescription"/> is for people and can change.
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