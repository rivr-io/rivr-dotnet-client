using System.Text.Json.Serialization;

namespace Rivr.Models.Orders;

public class CheckoutHint
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CheckoutHintType Type { get; set; }

    public string? Value { get; set; }
}