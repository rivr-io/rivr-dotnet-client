# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Security

- Exception messages no longer contain request or response bodies. `EnsureSuccessfulResponseAsync` used to put the full request body and the full response body in `HttpRequestException.Message`, and deserialisation errors put the content in `SerializationException.Message`. Bodies can contain personal data, such as a customer's personal identity number, and exception messages end up in logs and error trackers. The message now contains the status code, the HTTP method, the host, the request path without query string and, when available, a correlation id.
- The callback sample no longer logs the whole callback. It logs the callback's id, type and merchant, the parsed order status and selected fields from the order data.

### Added

- `RivrHttpRequestException`, an `HttpRequestException` thrown for unsuccessful responses other than 401 and 403, with `StatusCode`, `ReasonPhrase`, `Method`, `RequestPath`, `CorrelationId` and `ResponseContent`. `ResponseContent` is the response body: read it for the API's error details, do not log it.

- `IMerchantOperations.RefundAsync(Guid orderId, string? idempotencyKey, CancellationToken cancellationToken = default)` — refunds with an `Idempotency-Key` that you choose, so that a refund can be retried safely, also from a new call or a new process. The API stores the first successful response for 24 hours and returns it (header `Idempotent-Replayed: true`) for the same key and order instead of refunding again. A key that the API would refuse (not 1 to 255 printable ASCII characters, or only whitespace) throws `ArgumentException` before anything is sent, and `null` generates a key. It is an overload, so existing calls keep compiling and the existing method keeps its signature. Requires `Idempotency-Key` support on `POST /api/public/orders/{orderId}/refund` (RIV-3336); an API without it ignores the header. See "Retry a refund safely" in the README (RIV-3327).
- `IMerchantOperations.CancelAsync` — cancels an order that has not been paid and never refunds. When the order is not cancelled it throws `CancelOrderException` with `ErrorCode` (see `CancelOrderErrorCodes`), `StatusCode` and `IsRetryable`. Requires the `POST /api/public/orders/{orderId}/cancel` endpoint (RIV-2189).
- `ApiErrorResponse.ErrorCode`
- `RivrWebhookSignature` (`Rivr.Core.Webhooks`) — verifies the `Rivr-Signature` header on a callback: `IsValid(secret, rawBody, header)` recomputes the HMAC-SHA256 over `{t}.{rawBody}`, accepts the callback when any `v1` signature in the header matches (the header carries more than one while a key is rotated), compares in constant time and rejects a timestamp outside the tolerance (5 minutes by default). `ComputeSignature` produces the same value. Verify against the raw request body, not a re-serialised one.
- `IMerchantOperations.SendOrderToDeviceAsync` — sends an existing order in status `Created` to one of the merchant's devices (checkout terminals), the same action as "Send to terminal" in the Rivr portal. When the order is not sent it throws `SendOrderToDeviceException` with `StatusCode`, `PropertyName` and `IsRetryable` (409, the device is offline). Requires the `POST /api/public/orders/{orderId}/send-to-device` endpoint (RIV-1676).
- `CancellationToken` support for all async methods
- SourceLink support for debugging into source code
- Deterministic builds for reproducibility
- Symbol packages (.snupkg) for improved debugging experience

### Changed

- `RefundAsync(Guid orderId, CancellationToken cancellationToken)` now sends an `Idempotency-Key` header with a new GUID for every call. The key is set on the request, so a retry made by a handler in the `HttpClient` pipeline, for example a resilience handler, resends the same key and cannot refund twice. The library has no retry of its own, and a second call to `RefundAsync` gets a new key: pass your own key to retry across calls. Signature, behaviour and exceptions are unchanged (RIV-3327).
- The message of the exception thrown for an unsuccessful response has a new format and no longer contains bodies. Code that parsed the response body out of `Message` should read `RivrHttpRequestException.ResponseContent` instead. The exception is still an `HttpRequestException`.
- Renamed `SendHeartbeat` to `SendHeartbeatAsync` in `IPlatformOperations` for consistency
- Consolidated duplicate exception types (`UnauthorizedException`, `ForbiddenException`, `ErrorResponse`) into `Rivr.Core.Models`
- Improved exception handling: replaced generic `Exception` with `JsonException` for deserialization errors
- Updated nullable reference type patterns for better null safety

### Fixed

- README documentation now correctly references `AsOrOnBehalfOfMerchant()` method

## [1.x.x] - Previous Releases

For changes prior to this changelog, see the [GitHub releases](https://github.com/rivr-io/rivr-dotnet-client/releases).
