using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rivr.Extensions;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, string? url, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var content = CreateContent(value, options);
        return await client.PostAsync(url, content, cancellationToken);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<TValue>(this HttpClient client, string? url, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var content = CreateContent(value, options);
        return await client.PutAsync(url, content, cancellationToken);
    }

    private static StringContent CreateContent<TValue>(TValue value, JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Serialize(value, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return content;
    }
}