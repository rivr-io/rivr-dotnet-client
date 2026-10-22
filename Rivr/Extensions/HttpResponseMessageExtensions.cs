using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Rivr.Core.Models;

namespace Rivr.Extensions;

/// <summary>
/// Represents the operations that can be performed as a platform.
/// </summary>
public static class HttpResponseMessageExtensions
{
    internal const string CorrelationIdHeader = "X-Correlation-Id";
    private const string MaskedValue = "{value}";

    /// <summary>
    /// Ensures that the response is successful.
    /// </summary>
    /// <remarks>
    /// The exception message carries the status code, method, host, request path without query string and
    /// correlation id. It never carries the request or response body, since those can contain personal data and
    /// exception messages end up in logs. The response body is available in
    /// <see cref="RivrHttpRequestException.ResponseContent"/>.
    /// </remarks>
    /// <param name="message"></param>
    /// <exception cref="UnauthorizedException">The API responded 401.</exception>
    /// <exception cref="ForbiddenException">The API responded 403.</exception>
    /// <exception cref="RivrHttpRequestException">The API responded with any other unsuccessful status code.</exception>
    public static async Task EnsureSuccessfulResponseAsync(this HttpResponseMessage message)
    {
        if (message.IsSuccessStatusCode)
        {
            return;
        }

        if (message.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedException("Unauthorized. Please check your credentials.");
        }

        if (message.StatusCode == HttpStatusCode.Forbidden)
        {
            var errorResponse = await message.DeserialiseAsync<ErrorResponse>();

            if (errorResponse != null)
            {
                throw new ForbiddenException(errorResponse);
            }

            throw new ForbiddenException(new ErrorResponse
            {
                Error = "Forbidden",
                ErrorDescription = "The request is forbidden. Unknown reason."
            });
        }

        var method = message.RequestMessage?.Method.Method;
        var requestUri = message.RequestMessage?.RequestUri;
        var requestPath = requestUri is null ? null : RedactPath(requestUri);
        var correlationId = CorrelationIdOf(message);
        var responseContent = message.Content is null ? null : await message.Content.ReadAsStringAsync();

        throw new RivrHttpRequestException(
            DescribeFailure(message, method, requestUri, requestPath, correlationId),
            message.StatusCode,
            message.ReasonPhrase,
            method,
            requestPath,
            correlationId,
            responseContent);
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
            : throw new ArgumentNullException(nameof(message));
    }

    private static string DescribeFailure(
        HttpResponseMessage message,
        string? method,
        Uri? requestUri,
        string? requestPath,
        string? correlationId)
    {
        var builder = new StringBuilder()
            .Append("Response status code does not indicate success: ")
            .Append(((int)message.StatusCode).ToString(CultureInfo.InvariantCulture))
            .Append(" (").Append(message.ReasonPhrase).Append(").");

        if (method is not null && requestUri is not null)
        {
            builder.Append(' ').Append(method).Append(' ').Append(Authority(requestUri)).Append(requestPath).Append('.');
        }

        if (correlationId is not null)
        {
            builder.Append(" Correlation id: ").Append(correlationId).Append('.');
        }

        return builder.ToString();
    }

    private static string Authority(Uri uri) =>
        uri.IsAbsoluteUri ? uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped) : string.Empty;

    /// <summary>
    /// The path without query string or fragment. A segment is kept only when it is a GUID or a word (starts with
    /// a letter; letters, digits, '.', '_' and '-'; no run of five or more digits). Everything else, and the
    /// segment after a <c>by-*</c> segment, becomes <c>{value}</c>.
    /// </summary>
    internal static string RedactPath(Uri uri)
    {
        string path;
        if (uri.IsAbsoluteUri)
        {
            path = uri.AbsolutePath;
        }
        else
        {
            path = uri.OriginalString;
            var end = path.IndexOfAny(new[] { '?', '#' });
            if (end >= 0)
            {
                path = path.Substring(0, end);
            }
        }

        var segments = path.Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length == 0)
            {
                continue;
            }

            var segment = Uri.UnescapeDataString(segments[i]);
            var afterLookup = i > 0 && segments[i - 1].StartsWith("by-", StringComparison.OrdinalIgnoreCase);
            segments[i] = !afterLookup && IsSafeSegment(segment) ? segment : MaskedValue;
        }

        return string.Join("/", segments);
    }

    private static bool IsSafeSegment(string segment)
    {
        if (Guid.TryParse(segment, out _))
        {
            return true;
        }

        if (!IsAsciiLetter(segment[0]))
        {
            return false;
        }

        var digitRun = 0;
        foreach (var c in segment)
        {
            if (c is >= '0' and <= '9')
            {
                if (++digitRun >= 5)
                {
                    return false;
                }

                continue;
            }

            digitRun = 0;
            if (!IsAsciiLetter(c) && c is not ('.' or '_' or '-'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAsciiLetter(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    /// <summary>
    /// <c>X-Correlation-Id</c> from the response, then from the request, then the current activity's trace id.
    /// Only a GUID or a W3C trace id is accepted: the header is set by whoever sends the request and could
    /// otherwise carry anything into the message.
    /// </summary>
    internal static string? CorrelationIdOf(HttpResponseMessage message)
    {
        var candidates = new[]
        {
            HeaderValue(message.Headers, CorrelationIdHeader),
            HeaderValue(message.RequestMessage?.Headers, CorrelationIdHeader),
            Activity.Current?.TraceId.ToHexString(),
        };

        foreach (var candidate in candidates)
        {
            var value = candidate?.Trim();
            if (value is { Length: > 0 and <= 64 } && Guid.TryParse(value, out var parsed) && parsed != Guid.Empty)
            {
                return value;
            }
        }

        return null;
    }

    private static string? HeaderValue(HttpHeaders? headers, string name) =>
        headers is not null && headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
}
