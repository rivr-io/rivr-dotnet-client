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
}