using System;
using NUnit.Framework;
using Rivr.Core.Webhooks;
using Shouldly;

namespace Rivr.Test;

public class WebhookSignatureTests
{
    private const string Secret = "whsec_test_secret";
    private const long Ts = 1_700_000_000;
    private static readonly DateTimeOffset At = DateTimeOffset.FromUnixTimeSeconds(Ts);

    [Test]
    public void HeaderName_is_Rivr_Signature()
        => RivrWebhookSignature.HeaderName.ShouldBe("Rivr-Signature");

    [Test]
    public void ComputeSignature_matches_known_vector()
    {
        // Cross-impl golden vector — identical to the rivr-webhook service's WebhookSigner.
        var sig = RivrWebhookSignature.ComputeSignature(Secret, Ts, "{\"id\":\"evt_1\"}");

        sig.ShouldBe("248a374f50f943a28b0f6ab50faf9a7e7e29b710fa26df9fb1618b9bf8ea9c9a");
        sig.Length.ShouldBe(64);
    }

    [Test]
    public void IsValid_true_for_a_correctly_signed_payload()
    {
        const string body = "{\"eventId\":\"abc\",\"type\":\"order.completed\"}";
        var header = $"t={Ts},v1={RivrWebhookSignature.ComputeSignature(Secret, Ts, body)}";

        RivrWebhookSignature.IsValid(Secret, body, header, now: At).ShouldBeTrue();
    }

    [Test]
    public void IsValid_accepts_if_any_v1_matches()
    {
        // Multi-key header: a key we don't hold + the real one. Accept if ANY matches.
        const string body = "{\"a\":1}";
        var real = RivrWebhookSignature.ComputeSignature(Secret, Ts, body);
        var header = $"t={Ts},v1=00000000000000000000000000000000000000000000000000000000deadbeef,v1={real}";

        RivrWebhookSignature.IsValid(Secret, body, header, now: At).ShouldBeTrue();
    }

    [Test]
    public void IsValid_false_for_tampered_body()
    {
        var header = $"t={Ts},v1={RivrWebhookSignature.ComputeSignature(Secret, Ts, "{\"a\":1}")}";

        RivrWebhookSignature.IsValid(Secret, "{\"a\":2}", header, now: At).ShouldBeFalse();
    }

    [Test]
    public void IsValid_false_for_wrong_secret()
    {
        const string body = "{\"a\":1}";
        var header = $"t={Ts},v1={RivrWebhookSignature.ComputeSignature(Secret, Ts, body)}";

        RivrWebhookSignature.IsValid("whsec_other", body, header, now: At).ShouldBeFalse();
    }

    [Test]
    public void IsValid_false_for_stale_timestamp_beyond_tolerance()
    {
        const string body = "{\"a\":1}";
        var header = $"t={Ts},v1={RivrWebhookSignature.ComputeSignature(Secret, Ts, body)}";

        // "now" is 10 minutes after the signed timestamp, default tolerance is 5 minutes.
        RivrWebhookSignature.IsValid(Secret, body, header, now: At.AddMinutes(10)).ShouldBeFalse();
    }

    [Test]
    public void IsValid_true_for_old_timestamp_when_tolerance_is_widened()
    {
        const string body = "{\"a\":1}";
        var header = $"t={Ts},v1={RivrWebhookSignature.ComputeSignature(Secret, Ts, body)}";

        RivrWebhookSignature.IsValid(Secret, body, header, tolerance: TimeSpan.FromDays(3650), now: At.AddYears(2))
            .ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase("not-a-signature-header")]
    [TestCase("t=abc,v1=deadbeef")]   // unparseable timestamp
    [TestCase("t=1700000000")]        // no v1
    public void IsValid_false_for_malformed_header(string header)
        => RivrWebhookSignature.IsValid(Secret, "{\"a\":1}", header, now: At).ShouldBeFalse();

    [Test]
    public void IsValid_false_for_null_or_empty_secret()
        => RivrWebhookSignature.IsValid("", "{\"a\":1}", $"t={Ts},v1=deadbeef", now: At).ShouldBeFalse();
}
