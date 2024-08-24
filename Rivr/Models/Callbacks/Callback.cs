using System;

namespace Rivr.Models.Callbacks;

/// <summary>
/// Represents a callback.
/// </summary>
public class Callback
{
    public Guid Id { get; set; }
    public CallbackType Type { get; set; }
    public string? Status { get; set; }
    public string? Data { get; set; }
}