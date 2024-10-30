using System;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents a callback.
/// </summary>
public class Callback
{
    /// <summary>
    /// The ID of the callback.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type of the callback.
    /// </summary>
    public CallbackType Type { get; set; }

    /// <summary>
    /// The status of the callback.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// The data of the callback.
    /// </summary>
    public string Data { get; set; }
}