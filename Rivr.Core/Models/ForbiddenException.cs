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
    }

    /// <summary>
    /// The error code.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    public string? ErrorDescription { get; set; }
}