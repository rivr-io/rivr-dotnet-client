# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

Proposed version: **2.5.0** (minor). See "Breaking changes for implementers of `IMerchantOperations`" for why it is not a major.

### Breaking changes for implementers of `IMerchantOperations`

`IMerchantOperations` has a new member, `RefundAsync(Guid orderId, string? idempotencyKey, CancellationToken cancellationToken = default)`. A class of your own that implements the interface, such as a hand-written fake or stub in a test project or a wrapper that forwards to another `IMerchantOperations`, no longer compiles until it has the method (error CS0535). A custom implementation that is not recompiled against this version fails when it is loaded (`TypeLoadException`). `MerchantClient` has the method. The first two points below, keeping `Rivr` and `Rivr.Core` at the same version and the source compatibility of `RefundAsync`, concern every consumer of the packages, not only implementers.

- **Upgrade `Rivr` and `Rivr.Core` together.** `IMerchantOperations` is in `Rivr.Core`, and `MerchantClient`, the class that implements it, is in `Rivr`. A `Rivr` release requires `Rivr.Core` of the same version or later, not exactly the same version (`Rivr` 2.4.0 requires `Rivr.Core` 2.4.0 or later), so NuGet accepts `Rivr` 2.4.0 together with `Rivr.Core` 2.5.0, for example when a project references `Rivr.Core` directly or a central package version moves one package and not the other. The build passes, but the old `MerchantClient` does not implement the new member, and `Client` throws `TypeLoadException` ("Method 'RefundAsync' in type 'Rivr.MerchantClient' ... does not have an implementation") as soon as it is created, with `new Client(...)` or the first time `IClient` is resolved from the service provider. Nothing in the client works, not only refunds, and neither the compiler nor the restore warns about it. Keep `Rivr` and `Rivr.Core` at the same version.
- **Source compatibility of `RefundAsync`.** `RefundAsync` now has two overloads, which breaks three things that name the method without calling it. `var refund = merchant.RefundAsync;` (a method group with a natural delegate type, C# 10 and later) no longer compiles (CS8917, the delegate type could not be inferred); declare the delegate type instead, for example `Func<Guid, CancellationToken, Task> refund = merchant.RefundAsync;`. A documentation reference such as `<see cref="IMerchantOperations.RefundAsync"/>`, also on `MerchantClient`, must name the overload, `RefundAsync(Guid, CancellationToken)`: without it the compiler reports CS0419, which is a warning that becomes an error when warnings are treated as errors, and which only appears in a project that generates documentation. Reflection that looks the method up by name only, `GetMethod("RefundAsync")`, throws `AmbiguousMatchException` at run time; pass the parameter types. Ordinary calls, and a method group assigned to an explicit delegate type, are not affected.
- **Not affected, if `Rivr` and `Rivr.Core` are upgraded together:** code that only calls the interface (including `IClient.AsOrOnBehalfOfMerchant(...)`), mocks made with NSubstitute or Moq, and classes that derive from `MerchantClient`. Calls to `RefundAsync(orderId)` and `RefundAsync(orderId, cancellationToken)` compile as before, and a caller built against an earlier version still runs: the existing method is unchanged.
- **Fix:** add the method. A fake can answer it like its existing `RefundAsync`, and a wrapper forwards the call.
- **Not the first time:** 2.2.0 (`CancelAsync`) and 2.3.0 (`SendOrderToDeviceAsync`) also added a member to the interface, and the changelog did not say so. Upgrading from a version before 2.2.0 means adding all three.
- **Why a minor:** this is released as 2.5.0, like 2.2.0 and 2.3.0. The only implementation of the interface in this repository is `MerchantClient`, a custom implementation shows the break when it is compiled against the new version, and a default interface method, which would have avoided it, cannot be used because the package targets `netstandard2.0`. The mismatch between `Rivr` and `Rivr.Core` described above is not caught by the compiler, but a major version would not prevent it either, because it comes from upgrading one package and not the other. Strict SemVer would call a new interface member a major change (3.0.0). Choose 3.0.0 instead if you know of a consumer that implements the interface in production code.

### Added

- `IMerchantOperations.RefundAsync(Guid orderId, string? idempotencyKey, CancellationToken cancellationToken = default)` — refunds with an `Idempotency-Key` that you choose, so that a refund can be retried safely, also from a new call or a new process. The API stores the first successful response for 24 hours and returns it (header `Idempotent-Replayed: true`) for the same key and order instead of refunding again. A key that the API would refuse (not 1 to 255 printable ASCII characters, or only whitespace) throws `ArgumentException` before anything is sent, and `null` generates a key. It is an overload, so existing calls keep compiling and the existing method keeps its signature; adding it to the interface is what breaks custom implementations (see above). Requires `Idempotency-Key` support on `POST /api/public/orders/{orderId}/refund` (RIV-3336); an API without it ignores the header. See "Retry a refund safely" in the README (RIV-3327).

### Changed

- `RefundAsync(Guid orderId, CancellationToken cancellationToken)` now sends an `Idempotency-Key` header with a new GUID for every call, so every refund goes through the API's idempotency check (RIV-3336, RIV-3327). The signature, and the result of a refund that succeeds, are unchanged. The responses you can get and the effect of a retry are not:
  - **A retry in the `HttpClient` pipeline cannot refund twice.** The key is set once on the request, so a retry made by a handler in the pipeline, for example a resilience handler, resends the same key. It gets the stored response of the first attempt (header `Idempotent-Replayed: true`) instead of a second refund. If the first attempt is still being processed, the retry gets `409 idempotency_in_progress`, and your code gets it as a `RivrHttpRequestException` when the handler stops retrying. You cannot retry that call with the same key, because the key the library generates is not exposed, and a new call to `RefundAsync` gets a new key. The refund is most likely already in progress: check the order with `GetOrderAsync` before you call `RefundAsync` again, or pass your own key and retry with it.
  - **Separate calls do not see each other.** The library has no retry of its own, and a second call to `RefundAsync` gets a new key, so the API cannot tell that it is a retry. Two calls, also at the same time and for the same order, never get `409 idempotency_in_progress` or `422 idempotency_mismatch` from each other, because those answers need the same key. `422` needs the same key on another order (the library sends no request body and no query string). To retry across calls, pass your own key.
  - **New responses.** The endpoint gives these answers only when the header is present, so a call without your own key can now get them: `503 idempotency_unavailable` when the API cannot check the key (it fails closed and nothing is refunded; before, the refund ran without a check), `500 idempotency_outcome_unknown` for a failure after the refund has been started (the same key gets this answer again for 24 hours, so a retry in the pipeline does not start a second refund; before, it was a plain `500`), and `400 idempotency_key_invalid` for a token that identifies neither an API client nor a merchant. 409, 500 and 503 are thrown as `RivrHttpRequestException` (the `errorCode` is in `ResponseContent`) and 400 as `ApiCallException`, as before.
  - **A cancelled call can still refund.** With a key, the API finishes a refund that it has begun even if the client disconnects, so a call that ends with a timeout or an `OperationCanceledException` may have refunded the order. Check the order with `GetOrderAsync` before you call `RefundAsync` again, or pass your own key and retry with it.

  See "Retry a refund safely" in the README.

## [2.4.0] - 2026-10-22

### Added

- `RivrWebhookSignature` (`Rivr.Core.Webhooks`) — verifies the `Rivr-Signature` header on a callback: `IsValid(secret, rawBody, header)` recomputes the HMAC-SHA256 over `{t}.{rawBody}`, accepts the callback when any `v1` signature in the header matches (the header carries more than one while a key is rotated), compares in constant time and rejects a timestamp outside the tolerance (5 minutes by default). `ComputeSignature` produces the same value. Verify against the raw request body, not a re-serialised one.

## [2.3.0] - 2026-09-28

### Added

- `IMerchantOperations.SendOrderToDeviceAsync` — sends an existing order in status `Created` to one of the merchant's devices (checkout terminals), the same action as "Send to terminal" in the Rivr portal. When the order is not sent it throws `SendOrderToDeviceException` with `StatusCode`, `PropertyName` and `IsRetryable` (409, the device is offline). Requires the `POST /api/public/orders/{orderId}/send-to-device` endpoint (RIV-1676).

## [2.2.0] - 2026-09-25

### Security

- Exception messages no longer contain request or response bodies. `EnsureSuccessfulResponseAsync` used to put the full request body and the full response body in `HttpRequestException.Message`, and deserialisation errors put the content in `SerializationException.Message`. Bodies can contain personal data, such as a customer's personal identity number, and exception messages end up in logs and error trackers. The message now contains the status code, the HTTP method, the host, the request path without query string and, when available, a correlation id.
- The callback sample no longer logs the whole callback. It logs the callback's id, type and merchant, the parsed order status and selected fields from the order data.

### Added

- `RivrHttpRequestException`, an `HttpRequestException` thrown for unsuccessful responses other than 401 and 403, with `StatusCode`, `ReasonPhrase`, `Method`, `RequestPath`, `CorrelationId` and `ResponseContent`. `ResponseContent` is the response body: read it for the API's error details, do not log it.
- `IMerchantOperations.CancelAsync` — cancels an order that has not been paid and never refunds. When the order is not cancelled it throws `CancelOrderException` with `ErrorCode` (see `CancelOrderErrorCodes`), `StatusCode` and `IsRetryable`. Requires the `POST /api/public/orders/{orderId}/cancel` endpoint (RIV-2189).
- `ApiErrorResponse.ErrorCode`

### Changed

- The message of the exception thrown for an unsuccessful response has a new format and no longer contains bodies. Code that parsed the response body out of `Message` should read `RivrHttpRequestException.ResponseContent` instead. The exception is still an `HttpRequestException`.

## [1.2.0] - 2025-12-18

Entries written for 1.1.0 and 1.2.0, which were released the same day. They are not split further. Releases 1.2.1 to 2.1.0 have no entries in this file.

### Added

- `CancellationToken` support for all async methods
- SourceLink support for debugging into source code
- Deterministic builds for reproducibility
- Symbol packages (.snupkg) for improved debugging experience

### Changed

- Renamed `SendHeartbeat` to `SendHeartbeatAsync` in `IPlatformOperations` for consistency
- Consolidated duplicate exception types (`UnauthorizedException`, `ForbiddenException`, `ErrorResponse`) into `Rivr.Core.Models`
- Improved exception handling: replaced generic `Exception` with `JsonException` for deserialization errors
- Updated nullable reference type patterns for better null safety

### Fixed

- README documentation now correctly references `AsOrOnBehalfOfMerchant()` method

## [1.x.x] - Previous Releases

For changes prior to this changelog, see the [GitHub releases](https://github.com/rivr-io/rivr-dotnet-client/releases).
