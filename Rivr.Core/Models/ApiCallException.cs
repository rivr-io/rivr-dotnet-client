using System;

namespace Rivr.Core.Models;

/// <summary>
/// Represents and error that occurred during an API call.
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
}