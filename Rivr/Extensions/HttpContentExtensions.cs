using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rivr.Extensions;

public static class HttpContentExtensions
{
    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpContent content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        try
        {
            await using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(stream, options ?? new JsonSerializerOptions(), cancellationToken) ?? throw new Exception("Could not deserialise into the expected type");
        }
        catch (Exception e)
        {
            var response = await content.ReadAsStringAsync();
            throw new SerializationException($"Could not deserialise into the expected type. Response: {response}", e);
        }
    }
}