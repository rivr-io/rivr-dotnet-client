using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rivr.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpContent"/>.
/// </summary>
public static class HttpContentExtensions
{
    /// <summary>
    /// Deserialise the content of the HttpContent into the specified type.
    /// </summary>
    /// <remarks>
    /// When deserialisation fails the <see cref="SerializationException"/> does not contain the content, since it can
    /// contain personal data. The inner exception tells where in the JSON deserialisation failed.
    /// </remarks>
    /// <param name="content"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="SerializationException"></exception>
    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpContent content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        try
        {
            using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken) ?? throw new JsonException("Could not deserialize into the expected type");
        }
        catch (Exception e)
        {
            throw new SerializationException($"Could not deserialize the response into {typeof(T).Name}.", e);
        }
    }
}
