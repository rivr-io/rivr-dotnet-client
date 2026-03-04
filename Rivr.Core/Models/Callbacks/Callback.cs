using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rivr.Core.Models.Callbacks;

/// <summary>
/// Represents a callback.
/// </summary>
public class Callback
{
    /// <summary>
    /// The ID of the entity that the callback is for (e.g. Order ID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type of the callback.
    /// </summary>
    public CallbackType Type { get; set; }

    /// <summary>
    /// The ID of the merchant that the callback is for.
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// The status of the callback.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// The data of the callback, serialized as a JSON string.
    /// Accepts both a JSON string and a JSON object during deserialization.
    /// </summary>
    [JsonConverter(typeof(JsonObjectToStringConverter))]
    public string? Data { get; set; }
}

/// <summary>
/// Converts a JSON object or value to its string representation during deserialization,
/// enabling <see cref="Callback.Data"/> to accept both a raw JSON string and a nested JSON object.
/// </summary>
public class JsonObjectToStringConverter : JsonConverter<string?>
{
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.GetRawText();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}
