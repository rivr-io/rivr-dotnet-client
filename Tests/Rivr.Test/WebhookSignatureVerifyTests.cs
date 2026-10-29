using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rivr.Core.Webhooks;
using Shouldly;

namespace Rivr.Test;

/// <summary>
/// Tests of <see cref="WebhookSignature"/>, the verifier of the <c>Rivr-Signature</c> header.
/// </summary>
/// <remarks>
/// The test vectors in <c>TestData/webhook-signature-vectors.json</c> are a byte-for-byte copy of
/// <c>src/guides/examples/webhook-signature/vectors.json</c> in <c>rivr-io/rivr-developer-portal</c> (last changed in
/// commit 36732ec, copied from origin/main c0bfd87 on 2026-10-29). They are the vectors that the portal's guide
/// "Verify the webhook signature" publishes and that its Node, PHP, Python and .NET examples and the Node SDK are held
/// to in CI. They are made up: the secrets, ids and bodies are used nowhere else. When the portal adds a vector, copy
/// the file again and every test below that runs per vector proves it.
/// </remarks>
public class WebhookSignatureVerifyTests
{
    // The signing secret of the vector "one-key", read from the file, not written out here: a line that assigns a secret
    // to a variable is what the secret scan (hemlighetsgrinden) looks for, made up or not.
    private static string Secret => Named("one-key").Secrets[0];
    private const long SignedAt = 1789640080;
    private static readonly DateTimeOffset Clock = DateTimeOffset.FromUnixTimeSeconds(SignedAt + 12);
    private const string Body = "{\"id\":\"7b1e7c3a-2f4d-4a51-9c7e-5d0a6b8e2c14\",\"type\":\"Order\",\"status\":\"Completed\"}";

    public sealed record Vector(string Name, string? Header, string Body, string[] Secrets, long Now, bool Valid, Signing? Signing)
    {
        public DateTimeOffset At => DateTimeOffset.FromUnixTimeSeconds(Now);
    }

    public sealed record Signing(string[] Keys, long Timestamp, string? Body);

    private static readonly Lazy<(long ToleranceSeconds, IReadOnlyList<Vector> Vectors)> Loaded = new(Load);

    private static (long, IReadOnlyList<Vector>) Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "webhook-signature-vectors.json");
        using var document = JsonDocument.Parse(System.IO.File.ReadAllText(path));
        var root = document.RootElement;

        var vectors = root.GetProperty("vectors").EnumerateArray().Select(v =>
        {
            var headers = v.GetProperty("headers");
            Signing? signing = null;
            if (v.TryGetProperty("signing", out var s))
            {
                signing = new Signing(
                    s.GetProperty("keys").EnumerateArray().Select(k => k.GetString()!).ToArray(),
                    s.GetProperty("timestamp").GetInt64(),
                    s.TryGetProperty("body", out var b) ? b.GetString() : null);
            }

            return new Vector(
                v.GetProperty("name").GetString()!,
                headers.TryGetProperty("Rivr-Signature", out var h) ? h.GetString() : null,
                v.GetProperty("body").GetString()!,
                v.GetProperty("secrets").EnumerateArray().Select(x => x.GetString()!).ToArray(),
                v.GetProperty("now").GetInt64(),
                v.GetProperty("valid").GetBoolean(),
                signing);
        }).ToList();

        return (root.GetProperty("toleranceSeconds").GetInt64(), vectors);
    }

    private static IReadOnlyList<Vector> Vectors => Loaded.Value.Vectors;

    private static Vector Named(string name) => Vectors.Single(v => v.Name == name);

    public static IEnumerable<TestCaseData> AllVectors() =>
        Vectors.Select(v => new TestCaseData(v).SetArgDisplayNames(v.Name));

    public static IEnumerable<TestCaseData> VectorsWithOneSecret() =>
        Vectors.Where(v => v.Secrets.Length == 1).Select(v => new TestCaseData(v).SetArgDisplayNames(v.Name));

    public static IEnumerable<TestCaseData> VectorsRivrSigned() =>
        Vectors.Where(v => v.Signing is not null).Select(v => new TestCaseData(v).SetArgDisplayNames(v.Name));

    // How Rivr signs, written out again here and not taken from the code under test.
    private static string Sign(string secret, string timestamp, byte[] body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var message = Encoding.UTF8.GetBytes(timestamp + ".").Concat(body).ToArray();
        return Convert.ToHexString(hmac.ComputeHash(message)).ToLowerInvariant();
    }

    private static string Sign(string secret, long timestamp, string body) =>
        Sign(secret, timestamp.ToString(System.Globalization.CultureInfo.InvariantCulture), Encoding.UTF8.GetBytes(body));

    private static string HeaderFor(string secret, long timestamp, string body) =>
        $"t={timestamp},v1={Sign(secret, timestamp, body)}";

    [Test]
    public void The_vectors_are_found_and_are_the_ones_this_suite_was_written_against()
    {
        // A moved file must fail loudly, not run zero tests.
        Vectors.Count.ShouldBeGreaterThanOrEqualTo(17);
        Loaded.Value.ToleranceSeconds.ShouldBe((long)WebhookSignature.DefaultTolerance.TotalSeconds);
        Vectors.ShouldContain(v => v.Valid);
        Vectors.ShouldContain(v => !v.Valid);
    }

    [TestCaseSource(nameof(AllVectors))]
    public void Verify_with_a_string_body_answers_what_the_vector_says(Vector v) =>
        WebhookSignature.Verify(v.Body, v.Header, v.Secrets, now: v.At).ShouldBe(v.Valid);

    [TestCaseSource(nameof(AllVectors))]
    public void Verify_with_the_bytes_of_the_body_answers_what_the_vector_says(Vector v) =>
        WebhookSignature.Verify(Encoding.UTF8.GetBytes(v.Body), v.Header, v.Secrets, now: v.At).ShouldBe(v.Valid);

    [TestCaseSource(nameof(VectorsWithOneSecret))]
    public void Verify_with_one_secret_answers_as_with_a_list_holding_it(Vector v)
    {
        WebhookSignature.Verify(v.Body, v.Header, v.Secrets[0], now: v.At).ShouldBe(v.Valid);
        WebhookSignature.Verify(Encoding.UTF8.GetBytes(v.Body), v.Header, v.Secrets[0], now: v.At).ShouldBe(v.Valid);
    }

    [TestCaseSource(nameof(VectorsRivrSigned))]
    public void The_header_of_a_vector_is_what_Rivr_would_send_for_its_keys_timestamp_and_body(Vector v)
    {
        // The vectors prove the verifier; this proves the vectors, with the signing written out again in the test.
        var signing = v.Signing!;
        var signedBody = signing.Body ?? v.Body;
        var expected = $"t={signing.Timestamp}," + string.Join(",", signing.Keys.Select(k => "v1=" + Sign(k, signing.Timestamp, signedBody)));

        v.Header.ShouldBe(expected);
    }

    [Test]
    public void Verify_takes_a_header_that_came_more_than_once_as_ASP_NET_Core_joins_it()
    {
        var header = HeaderFor(Secret, SignedAt, Body);

        WebhookSignature.Verify(Body, "t=1,v1=00," + header, Secret, now: Clock).ShouldBeTrue();
    }

    [Test]
    public void Verify_uses_the_clock_when_it_is_not_given_a_time_and_the_default_tolerance()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        WebhookSignature.Verify(Body, HeaderFor(Secret, now, Body), Secret).ShouldBeTrue();
        WebhookSignature.Verify(Body, HeaderFor(Secret, now - 290, Body), Secret).ShouldBeTrue();
        WebhookSignature.Verify(Body, HeaderFor(Secret, now - 310, Body), Secret).ShouldBeFalse();
        WebhookSignature.Verify(Body, HeaderFor(Secret, now + 310, Body), Secret).ShouldBeFalse();
    }

    [Test]
    public void Verify_takes_the_tolerance_as_an_argument()
    {
        var tooOld = Named("too-old");
        WebhookSignature.Verify(tooOld.Body, tooOld.Header, tooOld.Secrets, now: tooOld.At).ShouldBeFalse();
        WebhookSignature.Verify(tooOld.Body, tooOld.Header, tooOld.Secrets, TimeSpan.FromSeconds(301), tooOld.At).ShouldBeTrue();

        var fresh = Named("one-key");
        WebhookSignature.Verify(fresh.Body, fresh.Header, fresh.Secrets, TimeSpan.FromSeconds(5), fresh.At).ShouldBeFalse();
        WebhookSignature.Verify(fresh.Body, fresh.Header, fresh.Secrets, TimeSpan.FromSeconds(12), fresh.At).ShouldBeTrue();
        WebhookSignature.Verify(fresh.Body, fresh.Header, fresh.Secrets, TimeSpan.FromSeconds(11.9), fresh.At).ShouldBeFalse();
    }

    [Test]
    public void Verify_with_zero_tolerance_accepts_only_the_second_of_the_timestamp()
    {
        var header = HeaderFor(Secret, SignedAt, Body);
        var exactly = DateTimeOffset.FromUnixTimeSeconds(SignedAt);

        WebhookSignature.Verify(Body, header, Secret, TimeSpan.Zero, exactly).ShouldBeTrue();
        WebhookSignature.Verify(Body, header, Secret, TimeSpan.Zero, exactly.AddSeconds(1)).ShouldBeFalse();
        WebhookSignature.Verify(Body, header, Secret, TimeSpan.Zero, exactly.AddSeconds(-1)).ShouldBeFalse();
    }

    [Test]
    public void Verify_does_not_trust_a_key_that_is_one_character_of_the_secret()
    {
        // Every secret starts with "w": a header signed with the key "w" would pass if the secret were walked as a list.
        var forged = HeaderFor("w", SignedAt, Body);

        WebhookSignature.Verify(Body, forged, Secret, now: Clock).ShouldBeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(",,,=,t=,v1=")]
    [TestCase("garbage")]
    [TestCase("t=1789640080")]
    [TestCase("t=99999999999999999999999999999999,v1=aa")]   // does not fit a long
    [TestCase("t=-1789640080,v1=aa")]
    [TestCase("t=+1789640080,v1=aa")]
    [TestCase("t=1789640080.5,v1=aa")]
    [TestCase("t=1 789640080,v1=aa")]
    [TestCase("t=١٧٨٩٦٤٠٠٨٠,v1=aa")]   // Arabic-Indic digits
    public void Verify_answers_false_and_does_not_throw_for_a_missing_or_broken_header(string? header)
    {
        WebhookSignature.Verify(Body, header, Secret, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Encoding.UTF8.GetBytes(Body), header, new[] { Secret }, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_does_not_accept_a_signature_of_another_length_or_in_capitals()
    {
        var good = Sign(Secret, SignedAt, Body);
        good.Length.ShouldBe(64);

        WebhookSignature.Verify(Body, $"t={SignedAt},v1={good[..63]}", Secret, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, $"t={SignedAt},v1={good}00", Secret, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, $"t={SignedAt},v1={good.ToUpperInvariant()}", Secret, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, $"t={SignedAt},v1={good}", Secret, now: Clock).ShouldBeTrue();
    }

    [Test]
    public void Verify_is_not_fooled_by_a_v1_among_others_or_by_spaces_around_the_parts()
    {
        var good = Sign(Secret, SignedAt, Body);
        var zeros = new string('0', 64);

        WebhookSignature.Verify(Body, $"t={SignedAt}, v1={zeros} , v1 = {good}", Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Body, $"v1={good},t={SignedAt}", Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Body, $"t={SignedAt},v2={good}", Secret, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, $"t={SignedAt},v1=", Secret, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_signs_t_as_it_is_written_in_the_header()
    {
        // Rivr never writes a leading zero. If one is there, the signature covers the text, not the number.
        var withZero = $"t=0{SignedAt},v1={Sign(Secret, "0" + SignedAt, Encoding.UTF8.GetBytes(Body))}";
        var numberOnly = $"t=0{SignedAt},v1={Sign(Secret, SignedAt, Body)}";

        WebhookSignature.Verify(Body, withZero, Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Body, numberOnly, Secret, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_skips_an_empty_secret_a_null_secret_and_an_empty_list()
    {
        var header = HeaderFor(Secret, SignedAt, Body);
        // The one thing an empty key does is sign: anyone can, so a header made with it must not verify.
        var madeWithEmptyKey = HeaderFor(string.Empty, SignedAt, Body);

        WebhookSignature.Verify(Body, madeWithEmptyKey, string.Empty, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, madeWithEmptyKey, new[] { string.Empty, string.Empty }, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, header, (string?)null, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, header, (IEnumerable<string?>?)null, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, header, Array.Empty<string>(), now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Body, header, new string?[] { null, string.Empty, Secret }, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Encoding.UTF8.GetBytes(Body), header, (IEnumerable<string?>?)null, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_checks_the_bytes_that_arrived_not_a_string_made_of_them()
    {
        // Not valid UTF-8, so it cannot be a string without being changed. The signature covers the bytes.
        var bytes = new byte[] { 0x7B, 0xFF, 0xFE, 0x7D };
        var header = $"t={SignedAt},v1={Sign(Secret, SignedAt.ToString(), bytes)}";

        WebhookSignature.Verify(bytes, header, Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(new byte[] { 0x7B, 0xFF, 0xFD, 0x7D }, header, Secret, now: Clock).ShouldBeFalse();
        WebhookSignature.Verify(Encoding.UTF8.GetString(bytes), header, Secret, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_reads_a_string_body_as_UTF_8()
    {
        const string body = "{\"visitType\":\"Återbesök\"}";
        var header = HeaderFor(Secret, SignedAt, body);

        WebhookSignature.Verify(body, header, Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Encoding.UTF8.GetBytes(body), header, Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Encoding.Latin1.GetBytes(body), header, Secret, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_accepts_an_empty_body_that_was_signed()
    {
        WebhookSignature.Verify(string.Empty, HeaderFor(Secret, SignedAt, string.Empty), Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(Array.Empty<byte>(), HeaderFor(Secret, SignedAt, string.Empty), Secret, now: Clock).ShouldBeTrue();
        WebhookSignature.Verify(string.Empty, HeaderFor(Secret, SignedAt, Body), Secret, now: Clock).ShouldBeFalse();
    }

    [Test]
    public void Verify_throws_for_what_only_the_callers_own_code_can_get_wrong()
    {
        var header = HeaderFor(Secret, SignedAt, Body);

        Should.Throw<ArgumentNullException>(() => WebhookSignature.Verify((string)null!, header, Secret, now: Clock))
            .ParamName.ShouldBe("payload");
        Should.Throw<ArgumentNullException>(() => WebhookSignature.Verify((byte[])null!, header, Secret, now: Clock))
            .ParamName.ShouldBe("payload");
        Should.Throw<ArgumentNullException>(() => WebhookSignature.Verify((string)null!, header, new[] { Secret }, now: Clock));
        Should.Throw<ArgumentNullException>(() => WebhookSignature.Verify((byte[])null!, header, new[] { Secret }, now: Clock));
        Should.Throw<ArgumentOutOfRangeException>(() => WebhookSignature.Verify(Body, header, Secret, TimeSpan.FromSeconds(-1), Clock))
            .ParamName.ShouldBe("tolerance");
        // Also when the header is missing: a wrong argument is wrong whatever the delivery looks like.
        Should.Throw<ArgumentOutOfRangeException>(() => WebhookSignature.Verify(Body, null, Secret, TimeSpan.FromTicks(-1), Clock));
    }

    [Test]
    public void The_constants_name_the_header_and_the_window_of_the_guide()
    {
        WebhookSignature.HeaderName.ShouldBe("Rivr-Signature");
        WebhookSignature.DefaultTolerance.ShouldBe(TimeSpan.FromMinutes(5));
    }
}
