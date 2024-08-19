namespace Rivr.Samples.CallbackHandler.Models;

public class Callback
{
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Data { get; set; }
}