using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rivr.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpRequestMessage"/>.
/// </summary>
public static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Deserialises the content of the <see cref="HttpRequestMessage"/> as the specified type.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpRequestMessage message, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        return message.Content is not null
            ? await message.Content.DeserialiseAsync<T>(options, cancellationToken)
            : throw new ArgumentNullException(nameof(message.Content));
    }
}