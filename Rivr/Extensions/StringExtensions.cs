using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rivr.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Deserialise the content of the string into the specified type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="options"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="SerializationException"></exception>
    public static T Deserialise<T>(this string? content, JsonSerializerOptions? options = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        try
        {
            return JsonSerializer.Deserialize<T>(content, options) ?? throw new JsonException("Could not deserialize into the expected type");
        }
        catch (Exception e)
        {
            throw new SerializationException($"Could not deserialize into the expected type. Content: {content}", e);
        }
    }
}