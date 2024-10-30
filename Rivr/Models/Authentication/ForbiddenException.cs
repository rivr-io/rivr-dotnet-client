using System;

namespace Rivr.Models.Authentication;

/// <summary>
/// This exception is thrown when the request is forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    /// <inheritdoc />
    public ForbiddenException(ErrorResponse message) : base(message.ErrorDescription)
    {
        Error = message.Error;
        ErrorDescription = message.ErrorDescription;
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