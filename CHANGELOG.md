# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
