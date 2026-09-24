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

- `IMerchantOperations.CancelAsync` — cancels an order that has not been paid and never refunds. When the order is not cancelled it throws `CancelOrderException` with `ErrorCode` (see `CancelOrderErrorCodes`), `StatusCode` and `IsRetryable`. Requires the `POST /api/public/orders/{orderId}/cancel` endpoint (RIV-2189).
- `ApiErrorResponse.ErrorCode`
- `CancellationToken` support for all async methods
- SourceLink support for debugging into source code
- Deterministic builds for reproducibility
- Symbol packages (.snupkg) for improved debugging experience

### Changed

- The message of the exception thrown for an unsuccessful response has a new format and no longer contains bodies. Code that parsed the response body out of `Message` should read `RivrHttpRequestException.ResponseContent` instead. The exception is still an `HttpRequestException`.
- Renamed `SendHeartbeat` to `SendHeartbeatAsync` in `IPlatformOperations` for consistency
- Consolidated duplicate exception types (`UnauthorizedException`, `ForbiddenException`, `ErrorResponse`) into `Rivr.Core.Models`
- Improved exception handling: replaced generic `Exception` with `JsonException` for deserialization errors
- Updated nullable reference type patterns for better null safety

### Fixed

- README documentation now correctly references `AsOrOnBehalfOfMerchant()` method

## [1.x.x] - Previous Releases

For changes prior to this changelog, see the [GitHub releases](https://github.com/rivr-io/rivr-dotnet-client/releases).
