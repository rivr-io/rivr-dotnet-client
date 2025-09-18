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
    /// <param name="content"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="SerializationException"></exception>
    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpContent content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        try
        {
            using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken) ?? throw new Exception("Could not deserialise into the expected type");
        }
        catch (Exception e)
        {
            var response = await content.ReadAsStringAsync();
            throw new SerializationException($"Could not deserialise into the expected type ({e.Message}) Response: {response}", e);
        }
    }
}