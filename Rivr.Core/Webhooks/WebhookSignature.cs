using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Rivr.Core.Webhooks
{
    /// <summary>
    /// Verifies the <c>Rivr-Signature</c> header of a webhook delivery, with the rules of the developer portal's guide
    /// "Verify the webhook signature" and its test vectors. The same rules apply to the clinic's <c>callbackUrl</c> and
    /// to the organisation's webhook endpoints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The header is <c>t=&lt;unix seconds&gt;,v1=&lt;hex&gt;[,v1=&lt;hex&gt;...]</c>, with one <c>v1</c> per active signing key.
    /// Each <c>v1</c> is the lowercase hex of <c>HMAC-SHA256</c> with the <b>whole</b> secret as the key (the UTF-8 bytes,
    /// <c>whsec_</c> included) over <c>"{t}.{raw body}"</c>. A delivery is accepted when one <c>v1</c> matches one of
    /// your secrets and <c>t</c> is within the tolerance of your clock, in either direction.
    /// </para>
    /// <para>
    /// Verify the body <b>exactly as it arrived</b>. JSON that has been parsed and written out again has other bytes
    /// (Rivr writes every character outside ASCII as a JSON escape), so model binding and <c>ReadFromJsonAsync</c> are
    /// not an option: read the request body as bytes and pass them in.
    /// </para>
    /// <para>
    /// This is the verifier to use. <see cref="RivrWebhookSignature.IsValid"/> is the older one, with one secret and a
    /// string body, and with less strict parsing of the header.
    /// </para>
    /// </remarks>
    public static class WebhookSignature
    {
        /// <summary>The HTTP header Rivr signs each webhook with.</summary>
        public const string HeaderName = RivrWebhookSignature.HeaderName;

        /// <summary>How far <c>t</c> may be from your clock, either way: 5 minutes.</summary>
        public static readonly TimeSpan DefaultTolerance = RivrWebhookSignature.DefaultTolerance;

        /// <summary>
        /// Whether a delivery really comes from Rivr: some <c>v1</c> in the header is the signature of
        /// <c>"{t}.{payload}"</c> under <paramref name="secret"/>, and <c>t</c> is within the tolerance of your clock.
        /// </summary>
        /// <param name="payload">
        /// The body exactly as it arrived. A string is taken as UTF-8: a body that is not valid UTF-8 cannot be a string,
        /// so use the <see cref="Verify(byte[], string, string, TimeSpan?, DateTimeOffset?)"/> overload when you can.
        /// </param>
        /// <param name="header">
        /// The <c>Rivr-Signature</c> header, or <see langword="null"/> when it is missing, which never verifies.
        /// ASP.NET Core's <c>Request.Headers["Rivr-Signature"]</c> can be passed as it is: a header that came more than
        /// once is joined with commas.
        /// </param>
        /// <param name="secret">
        /// Your signing secret, <c>whsec_</c> included. An empty one, such as a setting that exists but is not filled in,
        /// never verifies: anyone can sign with an empty key.
        /// </param>
        /// <param name="tolerance">How far <c>t</c> may be from your clock, either way. Default <see cref="DefaultTolerance"/>.</param>
        /// <param name="now">Your clock, for tests. The real clock when <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="false"/> for anything a caller on the network can send wrong: a missing or broken header, a
        /// <c>t</c> that is not a number, a body that was changed, another secret.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
        public static bool Verify(
            string payload,
            string? header,
            string? secret,
            TimeSpan? tolerance = null,
            DateTimeOffset? now = null)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            return VerifyCore(Encoding.UTF8.GetBytes(payload), header, new[] { secret }, tolerance, now);
        }

        /// <summary>
        /// Same as <see cref="Verify(string, string, string, TimeSpan?, DateTimeOffset?)"/> with several secrets: one
        /// <c>v1</c> that matches any of them is enough. Pass both while you rotate the key, and remove the old one
        /// once Rivr has stopped signing with it.
        /// </summary>
        /// <param name="payload">The body exactly as it arrived, read as a string (UTF-8).</param>
        /// <param name="header">The <c>Rivr-Signature</c> header, or <see langword="null"/> when it is missing.</param>
        /// <param name="secrets">
        /// Your signing secrets, <c>whsec_</c> included. Empty and <see langword="null"/> entries are skipped, and so is a
        /// <see langword="null"/> list: nothing verifies without a secret.
        /// </param>
        /// <param name="tolerance">How far <c>t</c> may be from your clock, either way. Default <see cref="DefaultTolerance"/>.</param>
        /// <param name="now">Your clock, for tests. The real clock when <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
        public static bool Verify(
            string payload,
            string? header,
            IEnumerable<string?>? secrets,
            TimeSpan? tolerance = null,
            DateTimeOffset? now = null)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            return VerifyCore(Encoding.UTF8.GetBytes(payload), header, secrets, tolerance, now);
        }

        /// <summary>
        /// Same as <see cref="Verify(string, string, string, TimeSpan?, DateTimeOffset?)"/>, with the body as the bytes
        /// that arrived. This is the safest overload: nothing is decoded and encoded again.
        /// </summary>
        /// <param name="payload">The body exactly as it arrived.</param>
        /// <param name="header">The <c>Rivr-Signature</c> header, or <see langword="null"/> when it is missing.</param>
        /// <param name="secret">Your signing secret, <c>whsec_</c> included. An empty one never verifies.</param>
        /// <param name="tolerance">How far <c>t</c> may be from your clock, either way. Default <see cref="DefaultTolerance"/>.</param>
        /// <param name="now">Your clock, for tests. The real clock when <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
        public static bool Verify(
            byte[] payload,
            string? header,
            string? secret,
            TimeSpan? tolerance = null,
            DateTimeOffset? now = null)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            return VerifyCore(payload, header, new[] { secret }, tolerance, now);
        }

        /// <summary>
        /// Same as <see cref="Verify(byte[], string, string, TimeSpan?, DateTimeOffset?)"/> with several secrets: one
        /// <c>v1</c> that matches any of them is enough.
        /// </summary>
        /// <param name="payload">The body exactly as it arrived.</param>
        /// <param name="header">The <c>Rivr-Signature</c> header, or <see langword="null"/> when it is missing.</param>
        /// <param name="secrets">
        /// Your signing secrets, <c>whsec_</c> included. Empty and <see langword="null"/> entries are skipped, and so is a
        /// <see langword="null"/> list.
        /// </param>
        /// <param name="tolerance">How far <c>t</c> may be from your clock, either way. Default <see cref="DefaultTolerance"/>.</param>
        /// <param name="now">Your clock, for tests. The real clock when <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
        public static bool Verify(
            byte[] payload,
            string? header,
            IEnumerable<string?>? secrets,
            TimeSpan? tolerance = null,
            DateTimeOffset? now = null)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            return VerifyCore(payload, header, secrets, tolerance, now);
        }

        private static bool VerifyCore(
            byte[] body,
            string? header,
            IEnumerable<string?>? secrets,
            TimeSpan? tolerance,
            DateTimeOffset? now)
        {
            var allowed = tolerance ?? DefaultTolerance;
            if (allowed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), "The tolerance must be zero or more.");
            }

            if (string.IsNullOrEmpty(header) || secrets == null)
            {
                return false;
            }

            // Split at commas and each part at its first '='. Keep t and every v1, skip names we don't know.
            string? timestamp = null;
            var signatures = new List<string>();
            foreach (var part in header!.Split(','))
            {
                var separator = part.IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                var name = part.Substring(0, separator).Trim();
                var value = part.Substring(separator + 1).Trim();
                if (name == "t")
                {
                    timestamp = value;
                }
                else if (name == "v1" && value.Length > 0)
                {
                    signatures.Add(value);
                }
            }

            if (timestamp == null || !IsAsciiDigits(timestamp) || signatures.Count == 0
                || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var signedAt))
            {
                return false;
            }

            // t is part of what is signed, so an old delivery cannot be replayed with a new t.
            var nowSeconds = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
            if (Math.Abs((double)signedAt - nowSeconds) > allowed.TotalSeconds)
            {
                return false;
            }

            // The message is t as it is written in the header, a dot, and the body's bytes.
            var prefix = Encoding.UTF8.GetBytes(timestamp + ".");
            var message = new byte[prefix.Length + body.Length];
            Buffer.BlockCopy(prefix, 0, message, 0, prefix.Length);
            Buffer.BlockCopy(body, 0, message, prefix.Length, body.Length);

            foreach (var secret in secrets)
            {
                // An empty secret is a key anyone can sign with.
                if (string.IsNullOrEmpty(secret))
                {
                    continue;
                }

                string expected;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    expected = ToLowerHex(hmac.ComputeHash(message));
                }

                foreach (var signature in signatures)
                {
                    if (FixedTimeEquals(expected, signature))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Only 0-9: char.IsDigit also accepts digits from other scripts, which the header never carries.
        private static bool IsAsciiDigits(string value)
        {
            if (value.Length == 0)
            {
                return false;
            }

            foreach (var c in value)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }

        private static string ToLowerHex(byte[] bytes)
        {
            var chars = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                chars[i * 2] = HexDigit(b >> 4);
                chars[i * 2 + 1] = HexDigit(b & 0xF);
            }

            return new string(chars);
        }

        private static char HexDigit(int nibble) => (char)(nibble < 10 ? '0' + nibble : 'a' + (nibble - 10));

        // Compares every character, so the time taken does not reveal how much of a guess was right. This is
        // CryptographicOperations.FixedTimeEquals by hand: the package targets .NET Standard 2.0, which does not have it.
        // The length of a signature is not a secret (64 hex characters), so a different length can return at once.
        private static bool FixedTimeEquals(string expected, string presented)
        {
            if (expected.Length != presented.Length)
            {
                return false;
            }

            var difference = 0;
            for (var i = 0; i < expected.Length; i++)
            {
                difference |= expected[i] ^ presented[i];
            }

            return difference == 0;
        }
    }
}
