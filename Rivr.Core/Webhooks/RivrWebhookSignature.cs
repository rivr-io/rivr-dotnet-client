using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Rivr.Core.Webhooks
{
    /// <summary>
    /// Verifies the <c>Rivr-Signature</c> header that Rivr puts on every outbound webhook, so an
    /// integrator can confirm a callback really came from Rivr and was not tampered with.
    /// </summary>
    /// <remarks>
    /// The header is <c>t=&lt;unixSeconds&gt;,v1=&lt;hex&gt;[,v1=&lt;hex&gt;…]</c>. Each <c>v1</c> is
    /// <c>HMAC-SHA256(key = UTF8(secret), message = UTF8("{t}.{rawBody}"))</c> as lowercase hex. A webhook
    /// may carry several <c>v1</c> values (a merchant key + a platform-wide key, and old+new during key
    /// rotation); accept the webhook if <em>any</em> of them matches a key you hold. Always verify against
    /// the <b>raw</b> request body (do not re-serialize the JSON first), and reject stale timestamps.
    /// </remarks>
    public static class RivrWebhookSignature
    {
        /// <summary>The HTTP header Rivr signs each webhook with.</summary>
        public const string HeaderName = "Rivr-Signature";

        /// <summary>Default allowed clock skew between the signed timestamp and now (replay protection).</summary>
        public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Computes the lowercase-hex HMAC-SHA256 signature over <c>"{timestamp}.{rawBody}"</c> — the value
        /// that appears as a <c>v1=</c> entry in the header. Useful for tests/diagnostics.
        /// </summary>
        public static string ComputeSignature(string secret, long timestampUnixSeconds, string rawBody)
        {
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            var signedPayload = timestampUnixSeconds.ToString(CultureInfo.InvariantCulture) + "." + (rawBody ?? string.Empty);
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                return ToHexLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload)));
            }
        }

        /// <summary>
        /// Returns true if the <paramref name="rivrSignatureHeader"/> is valid for the given
        /// <paramref name="secret"/> and <paramref name="rawBody"/>: the timestamp is within
        /// <paramref name="tolerance"/> (default 5 minutes) and at least one <c>v1</c> signature matches.
        /// Never throws on malformed input — returns false.
        /// </summary>
        /// <param name="secret">Your webhook signing secret (the <c>whsec_…</c> value).</param>
        /// <param name="rawBody">The exact raw request body, verbatim (not re-serialized).</param>
        /// <param name="rivrSignatureHeader">The value of the <c>Rivr-Signature</c> header.</param>
        /// <param name="tolerance">Allowed clock skew; defaults to <see cref="DefaultTolerance"/>.</param>
        /// <param name="now">Override for the current time (testing); defaults to UTC now.</param>
        public static bool IsValid(
            string secret,
            string rawBody,
            string rivrSignatureHeader,
            TimeSpan? tolerance = null,
            DateTimeOffset? now = null)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(rivrSignatureHeader))
            {
                return false;
            }

            long? timestamp = null;
            var signatures = new List<string>();
            foreach (var rawPart in rivrSignatureHeader.Split(','))
            {
                var part = rawPart.Trim();
                if (part.StartsWith("t=", StringComparison.Ordinal))
                {
                    if (long.TryParse(part.Substring(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        timestamp = parsed;
                    }
                }
                else if (part.StartsWith("v1=", StringComparison.Ordinal))
                {
                    signatures.Add(part.Substring(3));
                }
            }

            if (timestamp == null || signatures.Count == 0)
            {
                return false;
            }

            var skewSeconds = (long)(tolerance ?? DefaultTolerance).TotalSeconds;
            var nowSeconds = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
            if (Math.Abs(nowSeconds - timestamp.Value) > skewSeconds)
            {
                return false;
            }

            var expected = ComputeSignature(secret, timestamp.Value, rawBody);
            foreach (var candidate in signatures)
            {
                if (FixedTimeEquals(expected, candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ToHexLower(byte[] bytes)
        {
            var chars = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                chars[i * 2] = ToHexChar(b >> 4);
                chars[i * 2 + 1] = ToHexChar(b & 0xF);
            }

            return new string(chars);
        }

        private static char ToHexChar(int nibble) => (char)(nibble < 10 ? '0' + nibble : 'a' + (nibble - 10));

        // Constant-time string comparison (netstandard2.0 has no CryptographicOperations.FixedTimeEquals).
        // Both inputs are fixed-length lowercase hex, so comparing lengths up front does not leak useful info.
        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}
