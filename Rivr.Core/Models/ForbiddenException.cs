using System;

namespace Rivr.Core.Models;

/// <summary>
/// This exception is thrown when the request is forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    /// <inheritdoc />
    public ForbiddenException(string error, string errorDescription) : base(errorDescription)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// The error code.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    public string ErrorDescription { get; set; }
}