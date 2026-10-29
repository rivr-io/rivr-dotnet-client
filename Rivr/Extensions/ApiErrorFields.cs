using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Rivr.Core.Models;

namespace Rivr.Extensions;

/// <summary>
/// What an error answer says about itself, read so that it can be put on an exception: the API's <c>code</c> and
/// <c>type</c> from the body and the <c>Rivr-Request-Id</c> from the headers.
/// </summary>
/// <remarks>
/// Nothing here may fail or leak: the exception for the answer is thrown whatever the body looks like, and the values end
/// up in the integrator's logs. A value that is not shaped like what the API sends (a code is a short word, a type an
/// address, a request id a GUID) is dropped, so a proxy's error page or a changed body can't write free text, or line
/// breaks, into a property that is meant to be programmed against. None of the three can hold personal data.
/// </remarks>
internal static class ApiErrorFields
{
    internal const string RequestIdHeader = "Rivr-Request-Id";

    private const int MaxCodeLength = 64;
    private const int MaxTypeLength = 256;
    private const int MaxRequestIdLength = 128;

    /// <summary>
    /// <c>code</c> and <c>type</c> from a response body, or nothing for a body that is empty, not JSON or not an
    /// object. Names are matched without regard to case, like the rest of the client's deserialisation.
    /// </summary>
    internal static (string? Code, string? Type) ReadBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return default;
        }

        try
        {
            using var document = JsonDocument.Parse(body!);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return default;
            }

            string? code = null;
            string? type = null;
            foreach (var member in document.RootElement.EnumerateObject())
            {
                if (member.Value.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (string.Equals(member.Name, "code", StringComparison.OrdinalIgnoreCase))
                {
                    code = SafeCode(member.Value.GetString());
                }
                else if (string.Equals(member.Name, "type", StringComparison.OrdinalIgnoreCase))
                {
                    type = SafeType(member.Value.GetString());
                }
            }

            return (code, type);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// The same as <see cref="ReadBody"/>, for the body of a response. Never throws: a body that cannot be read (it is
    /// gone, or not text) has no code.
    /// </summary>
    internal static async Task<(string? Code, string? Type)> ReadBodyAsync(HttpResponseMessage message)
    {
        try
        {
            return message.Content is null ? default : ReadBody(await message.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
        catch (Exception)
        {
            // The caller is about to throw an exception for this response. It must be that exception, whatever the body is.
            return default;
        }
    }

    /// <summary>
    /// The <c>Rivr-Request-Id</c> header of a response: a GUID that identifies the call at Rivr. A value that is not 1 to
    /// 128 printable ASCII characters without spaces is dropped.
    /// </summary>
    internal static string? RequestIdOf(HttpResponseMessage message)
    {
        var value = HttpResponseMessageExtensions.HeaderValue(message.Headers, RequestIdHeader)?.Trim();
        return value is { Length: > 0 and <= MaxRequestIdLength } && IsPrintableAscii(value) ? value : null;
    }

    /// <summary>
    /// Takes <c>code</c> and <c>type</c> off an error that was deserialised as it is: what is not shaped like the API's
    /// values becomes <see langword="null"/>. Returns the same object.
    /// </summary>
    internal static ApiErrorResponse WithSafeFields(this ApiErrorResponse error)
    {
        error.Code = SafeCode(error.Code);
        error.Type = SafeType(error.Type);
        return error;
    }

    /// <inheritdoc cref="WithSafeFields(ApiErrorResponse)"/>
    internal static ErrorResponse WithSafeFields(this ErrorResponse error)
    {
        error.Code = SafeCode(error.Code);
        error.Type = SafeType(error.Type);
        return error;
    }

    // 1 to 64 of A-Z, a-z, 0-9, '_', '.', ':' and '-': what the API's codes and OAuth's error codes look like.
    internal static string? SafeCode(string? value)
    {
        if (value is not { Length: > 0 and <= MaxCodeLength })
        {
            return null;
        }

        foreach (var c in value)
        {
            var ok = c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '.' or ':' or '-';
            if (!ok)
            {
                return null;
            }
        }

        return value;
    }

    // An address: printable ASCII without spaces, so nothing that can break a log line.
    internal static string? SafeType(string? value) =>
        value is { Length: > 0 and <= MaxTypeLength } && IsPrintableAscii(value) ? value : null;

    private static bool IsPrintableAscii(string value) => value.All(c => c is >= '!' and <= '~');
}
