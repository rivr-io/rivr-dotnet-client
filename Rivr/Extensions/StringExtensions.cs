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
    /// <remarks>
    /// When deserialisation fails the <see cref="SerializationException"/> does not contain the content.
    /// </remarks>
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
            // The content is not included: a callback's data can contain personal data, and exception messages
            // end up in logs. The inner exception tells where in the JSON deserialisation failed.
            throw new SerializationException($"Could not deserialize the content into {typeof(T).Name}.", e);
        }
    }
}