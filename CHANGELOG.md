# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `CancellationToken` support for `GetHealthAsync()` method

### Changed

- Improved exception handling: replaced generic `Exception` with `JsonException` for deserialization errors
- Updated nullable reference type patterns for better null safety

### Fixed

- README documentation now correctly references `AsOrOnBehalfOfMerchant()` method

## [1.x.x] - Previous Releases

For changes prior to this changelog, see the [GitHub releases](https://github.com/rivr-io/rivr-dotnet-client/releases).
