using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rivr.Extensions;

public static class HttpRequestMessageExtensions
{
    public static async Task<T> DeserialiseAsync<T>([NotNull] this HttpRequestMessage message, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        return message.Content is not null
            ? await message.Content.DeserialiseAsync<T>(options, cancellationToken)
            : throw new ArgumentNullException(nameof(message.Content));
    }
}