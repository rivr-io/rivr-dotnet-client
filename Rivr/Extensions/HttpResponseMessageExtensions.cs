using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace Rivr.Extensions;

/// <summary>
/// Represents the operations that can be performed as a platform.
/// </summary>
public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Ensures that the response is successful.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="HttpRequestException"></exception>
    public static async Task EnsureSuccessfulResponseAsync(this HttpResponseMessage message)
    {
        if (message.IsSuccessStatusCode)
        {
            return;
        }

        var request = string.Empty;
        if (message.RequestMessage is { Content: not null })
        {
            request = await message.RequestMessage.Content.ReadAsStringAsync();
        }

        var response = await message.Content.ReadAsStringAsync();
        var errorMessage = $"Response status code does not indicate success: {message.StatusCode} ({message.ReasonPhrase}). Request: {request}, Response: {response}";

        throw new HttpRequestException(errorMessage);
    }

    /// <summary>
    /// Deserialise the content of the <see cref="HttpResponseMessage"/> as the specified type.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpResponseMessage message, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        return message.Content is not null
            ? await message.Content.DeserialiseAsync<T>(options, cancellationToken)
            : throw new ArgumentNullException(nameof(message.Content));
    }
}