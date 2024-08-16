using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace Rivr.Extensions;

public static class HttpResponseMessageExtensions
{
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

    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpResponseMessage message, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        return message.Content is not null
            ? await message.Content.DeserialiseAsync<T>(options, cancellationToken)
            : throw new ArgumentNullException(nameof(message.Content));
    }
}