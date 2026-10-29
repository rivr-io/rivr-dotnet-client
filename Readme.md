# Rivr .NET Client

[![nuget](https://img.shields.io/nuget/v/rivr.svg)](https://www.nuget.org/packages/rivr/) [![nuget](https://img.shields.io/nuget/dt/rivr.svg)](https://www.nuget.org/packages/rivr/) [![Build and tests](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml/badge.svg)](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml)

## Overview

This is an API Client library for Rivr Order API. This library enables an application to send orders for processing.

### Compatibility

This library is built with support for [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) which provides the following compatibility:

| .NET implementation | Version support                                  |
| ------------------- | ------------------------------------------------ |
| .NET and .NET Core  | 2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0, 7.0, 8.0, 9.0 |
| .NET Framework      | 4.6.1, 4.6.2, 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1      |

If you require support for and earlier version of .NET Framework, please feel free to reach out to support@rivr.io to see what we can do.

### Contribute

If you see something that can be improved, feel free to raise an issue or submit a pull request at [Github](https://github.com/rivr-io/rivr-dotnet-client).

### Installation

This libary is published on [Nuget](https://www.nuget.org/packages/Rivr). Use your favourite package manager to add it to your project.

```
dotnet add package Rivr
```

### Security

The industry-standard [OAuth2](https://oauth.net/2/) security protocol is used for authorization.

#### Credentials

You will need a `client_id` and `client_secret`. Reach out to support@rivr.io and we will setup your application and provide you with the credentials you need.

#### Security contexts

A consumer of this library will be able to perform actions as a platform or on behalf of the managed merchants. While using this library, we have taken care of the authentication for you along with caching of access tokens.

##### Platform level

The platform level authentication utilizes a simplified version of OAuth2 and uses `grant_type = client_credentials`, which uses `client_id` and `client_secret` to fetch an `access_token`.

##### Merchant level

The merchant level authentication is a variant of OAuth2 which allows the system to act on behalf of a merchant. It is very similar to the platform level authentication, but uses our own `grant_type = merchant_token`. With this, the `client_id` and the `client_secret` along with a `merchant_id` are used to fetch an `access_token` that includes the `merchant_id`. This allows for the platform to perform actions on behalf of the merchant.

## Using the client

The following section describes how to use this library.

### Creating an instance of the client

```C#
var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
var memoryCache = new MemoryCache(new MemoryCacheOptions());

var client = new Client(config, memoryCache);
```

### Using Dependency Injection

```C#
await Host.CreateDefaultBuilder(args).ConfigureServices(
        (context, services) =>
        {
            services.AddRivrClient(context.Configuration, configBuilder =>
            {
                /*

                To run service, you need to provide your client ID and client secret.

                1. Either use explicit configuration like this:

                configBuilder.UseClientId("enter-client-id-here");
                configBuilder.UseClientSecret("enter-client-secret-here");
                configBuilder.UseTestEnvironment();

                2. Or add the following to your appSettings.json:

                "Rivr": {
                    "ClientId": "enter-client-id-here",
                    "ClientSecret": "enter-client-secret-here",
                    "Environment": "Test"
                }

                The explicit configuration will take precedence.

                */
            });
            services.AddHostedService<MyService>();
        }
    )
    .RunConsoleAsync();
```

### Operations

#### Check API health (unauthenticated)

Check that the API is online and operational using the `GetHealthAsync` method.

```C#
var result = await client.GetHealthAsync();
// result.Message is "OK"
```

### Platform operations

These operations are performed as a platform.

#### Check API health (using authentication)

Check that the API is online and operational using the `GetHealthSecureAsync` method. This is also a way to test that the authentication works as intended.

```C#
var result = await client
    .AsPlatform()
    .GetHealthSecureAsync();
// result.Message is "OK"
```

#### Fetch merchants

Fetch a list of the merchants the platform is configured to act on behalf of.

```C#
var merchants = await client
    .AsPlatform()
    .GetMerchantsAsync();
// merchants contains an array of Merchant objects
```

### Merchant operations

Using the merchantId's received in above mentioned operation, a number of operations can be performed on behalf of a merchant.

####

#### Check API health (using authentication)

Check that the API is online and operational using the `GetHealthSecureAsync` method. This is also a way to test that the authentication for the merchant works as intended.

```C#
var merchantId = Guid.Parse("...");
var result = await client
    .AsOrOnBehalfOfMerchant(merchantId)
    .GetHealthSecureAsync();
// result.Message is "OK"
```

#### Fetch devices for a merchant

A merchant can be configured to have zero or more devices (checkout terminals). If a payment is to be sent to a specific device, the deviceId must be added to the order creation. Use this operation to fetch all the devices that the merchant has.

```C#
var devices = await client
    .AsOrOnBehalfOfMerchant(sdkMerchantId)
    .GetDevicesAsync();
// devices contains an array of Device objects
```

#### Create order (payment)

The creation of an order marks the beginning of the customer journey. The `CreateOrderAsync` operation uses the HTTP `PUT` VERB using `order.Id` as the resource identifier. This means that the operation is idempotent. Subsequent calls using `CreateOrderAsync` with the same `order.Id` will result in one `Order`. While this operation does support updates, the purpose of the idempotency is to enable at-least-once delivery. This is not meant to be used as a mechanism where the API keeps state for a client system. This is because the merchant may have configured its checkout to notify customers immediately which in turn means that the customer may have started to interact with the order. A completed order cannot be updated.

```C#
// Minimum order information
var order = new CreateOrderRequest
{
    Id = Guid.NewGuid(),
    PersonalNumber = "190001010101",
    Email = "test@example.com",
    Phone = "0700000000",
    Reference = "123456",
    OrderLines =
    [
        new OrderLine
        {
            Description = "Test Product",
            Quantity = 1,
            UnitPriceExclVat = 100,
            VatPercentage = 0
        }
    ]
};
// Optional order information
order.CheckoutHints =
[
    new CheckoutHint
    {
        Type = CheckoutHintType.Device,
        Value = "deviceId"
    }
];
order.Metadata = new Dictionary<string, string>
{
    { "key1", "value1" },
    { "key2", "value2" }
};
order.CallbackUrl = "https://example.com/callback";

var merchantId = Guid.Parse("...");
var result = await client
    .AsOrOnBehalfOfMerchant(merchantId)
    .CreateOrderAsync(order);
// result contains an instance of an Order object
```

#### Send an order to a device

`SendOrderToDeviceAsync` sends an existing order to one of the merchant's devices (checkout terminals) so the customer can pay on it — the same action as "Send to terminal" in the Rivr portal. Use it for an order that was created without a device checkout hint, to send it again, or to send it to another device. The order must be in status `OrderStatus.Created`, and the device must belong to the merchant and be online (`Device.IsOnline` from `GetDevicesAsync`). Delivery is asynchronous; the payment result is reported through the order status and the order callback.

When the order is not sent a `SendOrderToDeviceException` is thrown. HTTP 400 means the order is not in status `Created` or the device is not one of the merchant's devices; 404 means the order was not found; 409 means the device is offline, and `IsRetryable` is `true`.

```C#
var merchantId = Guid.Parse("...");
try
{
    await client
        .AsOrOnBehalfOfMerchant(merchantId)
        .SendOrderToDeviceAsync(order.Id, device.Id);
}
catch (SendOrderToDeviceException e) when (e.IsRetryable)
{
    // The device is offline. Try again when it is online.
}
```

#### Fetch an order

Using the `order.Id`, the order can be retrieved.

```C#
var merchantId = Guid.Parse("...");
var result = await client
    .AsOrOnBehalfOfMerchant(merchantId)
    .GetOrderAsync(order.Id);
// result contains an instance of an Order object
```

#### Refund or Cancel an order

An order can be either refunded or cancelled depending on the current status. If the status is `OrderStatus.Completed`, the order will be refunded. If the status is `OrderStatus.Created`, the order will be cancelled. The calling system does not need to know the current status of the order.

Note: There are certain payment methods that do not support refunds (some instalment products).

```C#
var merchantId = Guid.Parse("...");
await client
    .AsOrOnBehalfOfMerchant(merchantId)
    .RefundAsync(order.Id);
```

#### Retry a refund safely

A refund moves money, so a refund that may or may not have gone through must never be repeated blindly. `RefundAsync` therefore sends an `Idempotency-Key` header with every request, and the API uses it to recognise a retry: a request with the same key for the same order gets the stored response of the first request (with the header `Idempotent-Replayed: true`) instead of a second refund. A successful response is stored for 24 hours. The key is scoped to your API client and to the refund endpoint, and a key that has been used for another order is refused (422). An API version that does not know the header ignores it.

- **Without a key from you**, `RefundAsync(order.Id)` generates a new GUID for every call. The library does not retry by itself, but if something in the HTTP pipeline does, for example a resilience handler that you add to the `ApiClient` named `HttpClient`, the retry resends the same request with the same key and cannot refund twice. **Calling `RefundAsync` again is a new call with a new key**: the API cannot see that it is a retry.
- **With your own key**, retries from your own code, after a timeout, a crash or a restart, are safe. Generate the key once, store it with the refund request in your own system before the first attempt, and pass it every time.

```C#
// Generated once, and stored with the refund request before the first attempt.
var idempotencyKey = Guid.NewGuid().ToString();

var merchantId = Guid.Parse("...");
await client
    .AsOrOnBehalfOfMerchant(merchantId)
    .RefundAsync(order.Id, idempotencyKey);
// With a cancellation token: RefundAsync(order.Id, idempotencyKey, cancellationToken)
```

The key is 1 to 255 printable ASCII characters (a UUID is recommended), it must be random and it must not contain personal data. `RefundAsync` throws `ArgumentException` for a key that the API would refuse, before anything is sent; `null` means "generate one".

What the API answers, with the same key:

| HTTP | `errorCode`                   | Meaning                                                                                     | What to do                                                                                                        |
| ---- | ----------------------------- | ------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| 409  | `idempotency_in_progress`     | A request with the same key is still being processed.                                       | Retry shortly with the same key. If it lasts more than a few minutes, check the order before you use a new key.   |
| 503  | `idempotency_unavailable`     | The key could not be checked, so nothing was done.                                          | Retry with the same key.                                                                                          |
| 422  | `idempotency_mismatch`        | The key has already been used for another order or another request. Nothing was done.       | Use a new key for every refund.                                                                                   |
| 500  | `idempotency_outcome_unknown` | The request failed after the refund may have been started. The same key gives this answer for 24 hours. | Check the order (`GetOrderAsync`) before you retry with a new key.                                      |
| 400  | `idempotency_key_invalid`     | The API did not accept the key.                                                             | Use a valid key. The library catches this before sending, so you should only see it for a token the API cannot scope a key to. |

409, 422, 500 and 503 are thrown as `RivrHttpRequestException`: `StatusCode` tells them apart and `ResponseContent` holds the `errorCode`. 400 is thrown as `ApiCallException`, like every other bad request from `RefundAsync`. A 409 can also be one of the temporary refusals described for `CancelAsync` (`payment_in_progress`, `order_changed`) or `claim_check_unavailable`: retrying with the same key is right for all of them.

```C#
try
{
    await client
        .AsOrOnBehalfOfMerchant(merchantId)
        .RefundAsync(order.Id, idempotencyKey);
}
catch (RivrHttpRequestException e) when (e.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.ServiceUnavailable)
{
    // Wait a moment and call RefundAsync again with the same idempotencyKey.
}
```

#### Cancel an order

`CancelAsync` cancels an order that has not been paid. Unlike `RefundAsync` it never refunds anything: if the order can no longer be cancelled — typically because the customer has already paid — nothing happens and a `CancelOrderException` is thrown. Its `ErrorCode` says what to do (see `CancelOrderErrorCodes`), and `IsRetryable` is `true` when the same call can succeed later. Cancelling an order that is already cancelled succeeds.

| HTTP | `ErrorCode`             | Meaning                                                                            |
| ---- | ----------------------- | ---------------------------------------------------------------------------------- |
| 409  | `payment_in_progress`   | A payment is in progress right now, for example a card payment on a terminal. Retry. |
| 409  | `order_changed`         | The order changed while it was being cancelled. Retry.                             |
| 400  | `order_paid`            | The order has been paid. Use `RefundAsync` instead.                                |
| 400  | `payment_pending`       | A payment has been started and can no longer be stopped. Follow the order status.  |
| 400  | `order_refunded`        | The order has already been refunded.                                               |
| 400  | `order_not_cancellable` | The order is in a status that cannot be cancelled.                                 |
| 404  | `order_not_found`       | There is no order with the given id.                                               |

The codes say what to do, not which payment method is involved; the exception message carries the details.

```C#
var merchantId = Guid.Parse("...");
try
{
    await client
        .AsOrOnBehalfOfMerchant(merchantId)
        .CancelAsync(order.Id);
}
catch (CancelOrderException e) when (e.ErrorCode == CancelOrderErrorCodes.OrderPaid)
{
    // The customer has already paid. Refund the order if that is what you want.
}
catch (CancelOrderException e) when (e.IsRetryable)
{
    // A payment is in progress or the order changed. Try again shortly.
}
```

### Error handling

| Response | Exception |
| -------- | --------- |
| 401 | `UnauthorizedException` |
| 403 | `ForbiddenException` (`Error`, `ErrorDescription`) |
| Other unsuccessful status codes | `RivrHttpRequestException`, an `HttpRequestException` |

The message of a `RivrHttpRequestException` contains the status code, the HTTP method, the host, the request path without query string and, when available, a correlation id:

```text
Response status code does not indicate success: 404 (Not Found). GET https://api.rivr.io/api/public/orders/1f3e2b7a-5c0d-4d8e-9a41-2b6f0c9d7e15. Correlation id: 4bf92f35-77b3-4da6-a3ce-929d0e0e4736.
```

It never contains the request or response body. Bodies can contain personal data, such as a customer's personal identity number, and exception messages end up in your logs. When you need the API's error details, read the properties:

```C#
catch (RivrHttpRequestException e) when (e.StatusCode == HttpStatusCode.BadRequest)
{
    // e.ResponseContent is the response body. It may contain personal data: do not log it.
    // e.CorrelationId identifies the request if you contact Rivr support.
}
```

Because the library targets .NET Standard 2.0, the base class's `HttpRequestException.StatusCode` is not set on .NET 5 and later. Use `RivrHttpRequestException.StatusCode`.

#### Program against `Code`

Every error answer under `/api/public` carries a stable `code` (for example `order_paid`, `token_expired` or `device_offline`) and a `type`, the address of the code's page in the developer portal: `https://developers.rivr.io/errors/{code}`. Since 2026-10-25 that holds for all of them: the token, the scope, a request that cannot be read and every operation's own errors. Answers from rivr-public-api also have the header `Rivr-Request-Id`, a GUID that identifies the call at Rivr.

The exceptions expose all three, next to what they exposed before: `Code`, `Type` and `RequestId`. They are on `UnauthorizedException`, `ForbiddenException`, `RivrHttpRequestException`, `ApiCallException` (the 400 of `RefundAsync` is one) and its subclasses `CancelOrderException` and `SendOrderToDeviceException`.

- **Decide what to do from `Code`.** `Message`, `ErrorDescription` and the text in `ResponseContent` are for people and can change; a code stays. New codes come over time, so keep a branch for a code you do not know, and take the status code as the broad answer.
- **`Code` is `null` when the answer has none**: an API version from before 2026-10-25, the token endpoint (it answers with OAuth's `error`) or a gateway's own error page. `RequestId` is `null` when the answer has no `Rivr-Request-Id`. A value that does not look like what the API sends (a code is a short word) is not passed on, so the properties are safe to log.
- **Log `RequestId` with the error and quote it when you contact Rivr support.** It is not `RivrHttpRequestException.CorrelationId`, which is the `X-Correlation-Id` of the request. The exception messages are as they were; the request id has its own property.
- `ErrorCode` on `CancelOrderException` is still there and has the same value as `Code` where the answer has both. Use `Code`: it is there on every error.

```C#
try
{
    await client
        .AsOrOnBehalfOfMerchant(merchantId)
        .RefundAsync(order.Id, idempotencyKey);
}
catch (ApiCallException e) when (e.Code == "order_not_refundable")
{
    // 400: the order cannot be refunded. Nothing was done.
}
catch (RivrHttpRequestException e) when (e.Code == "idempotency_outcome_unknown")
{
    // Check the order with GetOrderAsync before you refund again with a new key.
}
catch (RivrHttpRequestException e) when (e.Code is "payment_in_progress" or "order_changed" or "claim_check_unavailable" or "idempotency_in_progress" or "idempotency_unavailable")
{
    // Temporary: call RefundAsync again with the same idempotencyKey.
}
catch (RivrHttpRequestException e)
{
    // Log what identifies the failure, never e.ResponseContent.
    logger.LogError("Refund failed: {Status} {Code}, Rivr-Request-Id {RequestId}", e.StatusCode, e.Code, e.RequestId);
    throw;
}
```

The same applies to callbacks: log the callback's id and type, not the callback or its `Data`. See `Samples/Rivr.Samples.CallbackHandler`.

## Callbacks

The integration is based on the calling system being able to receive callbacks (webhooks). The callback contains the orderId and the `OrderStatus` describing the event that occurred. Depending on the event, there may be more information provided. The callback is delivered to the `callbackUrl` that is set when creating the order.

Example of a callback body:

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Created",
  "data": {
    "CreatedDate": "2024-08-19T14:58:21Z",
    "Metadata": {}
  }
}
```

If the system is unable to receive callbacks, the `GetOrderAsync` operation can be used periodically to check the status of an order.

### Callback details

Callbacks from the Rivr systems are delivered using a HTTP POST to the URL supplied in the initial creation of the object.

If the request is receiving a response having HTTP Status Code in the range 200-299 it will be considered successful and no more attempts will be made.

However, if the callback isn't successful, a retry will be scheduled. A maximum of seven retries will be performed according to this schema:

| Iteration | Delay      |
| --------- | ---------- |
| 1         | 1 minute   |
| 2         | 5 minutes  |
| 3         | 30 minutes |
| 4         | 60 minutes |
| 5         | 4 hours    |
| 6         | 12 hours   |
| 7         | 24 hours   |

If the last iteration is not successful, no more attempts will be made.

### Verifying the callback signature

Every callback is signed so you can confirm it really came from Rivr. The request carries a
`Rivr-Signature` header of the form `t=<unixSeconds>,v1=<hex>[,v1=<hex>…]`, where each `v1` is
`HMAC-SHA256` over `"{t}.{rawBody}"` (lowercase hex). A callback may carry several `v1` values (a
merchant key plus a platform-wide key, and old + new during key rotation) — accept it if **any** of
them matches a key you hold.

Use `WebhookSignature.Verify` (in `Rivr.Core.Webhooks`) from your callback handler. It follows the rules
of the developer portal's guide "Verify the webhook signature" and passes the guide's test vectors, and it
works the same for the `callbackUrl` and for your organisation's webhook endpoints. Pass the body **as the
bytes that arrived**: JSON that has been parsed and written out again has other bytes (Rivr writes every
character outside ASCII as a JSON escape), so model binding such as `[FromBody]` is not an option here.

```csharp
using Rivr.Core.Webhooks;

[HttpPost("callbacks/rivr")]
public async Task<IActionResult> Handle()
{
    // The raw body, read before anything parses it.
    using var buffer = new MemoryStream();
    await Request.Body.CopyToAsync(buffer);
    var rawBody = buffer.ToArray();

    // _webhookSecrets: your signing secret(s), whsec_ prefix included.
    if (!WebhookSignature.Verify(rawBody, Request.Headers["Rivr-Signature"], _webhookSecrets))
    {
        return Unauthorized(); // not 400: Rivr never retries a 400, but it retries a 401 on schedule
    }

    // Rivr-Webhook-Id is the delivery's id, the same on every retry. Store it under a unique key in the same
    // transaction as the work and skip an id you have seen. It is not part of the signature: verify first.
    string? webhookId = Request.Headers["Rivr-Webhook-Id"];

    // ...deserialize rawBody and handle the event
    return Ok(); // within 5 seconds
}
```

- **What counts as valid.** The header is `t=<unix seconds>,v1=<hex>[,v1=<hex>…]`. `Verify` is `true` when one `v1`
  is the `HMAC-SHA256` of `"{t}.{raw body}"` under one of your secrets (the **whole** secret is the key, `whsec_`
  included) and `t` is within 5 minutes of your clock, in either direction. Pass another window as the
  `tolerance` argument. The comparison takes the same time whatever the guess.
- **Several secrets.** Pass them as a list while you rotate a key; one match is enough. A delivery on the
  `callbackUrl` path carries one `v1` per active key on the clinic and on its platform, and you can hold
  just one of them. Remove the old secret once Rivr has stopped signing with it.
- **It answers `false`, it does not throw,** for anything a caller on the network can send wrong: no header, a
  broken header, a body that was changed, a timestamp that is not a number, another key. An empty secret, such
  as a setting that exists but is not filled in, never verifies (anyone can sign with an empty key); a `null`
  secret does not either. It throws `ArgumentNullException` for a `null` body and `ArgumentOutOfRangeException`
  for a negative tolerance, since only your own code gets those wrong.
- **No header.** On the `callbackUrl` path signing is optional: a delivery arrives unsigned when no signing key
  is active, and `Verify` answers `false` for it. Decide what you do with that on purpose. Organisation
  endpoints are always signed.
- **A body as a string.** `Verify` also takes the body as a `string`, read as UTF-8. Prefer the bytes: they are
  exactly what was signed.
- **Tests.** The guide publishes made-up test vectors. Put them in your own test suite, with `now` set to the
  vector's clock (`Verify(body, header, secrets, now: ...)`); this library's tests run all of them.

`RivrWebhookSignature.IsValid` (one secret, a string body) is still there and behaves as before. New code
should use `WebhookSignature.Verify`. Manage your webhook signing keys in the Rivr portal.

Note: If the Callback URL is empty or malformatted there will be no attempts at all.

#### Order

Here follows a detailed overview of the callbacks related to the Order entity.

##### Created

Immediately after an order is created a callback is issued.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Created",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "Metadata": {}
  }
}
```

##### Pending

If an order is transferred to an invoice with recourse, the order state is set to pending meaning that there has been an action on it but it is not completed yet. The order will be completed when the invoice is paid in full.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Pending",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "Metadata": {}
  }
}
```

##### Completed

A order is considered `Completed` when it has been paid. An Order can be paid with a different payment methods. The different variants of callbacks are listed here.

###### Card

Includes Apple Pay and Google Pay which are also reported as `Card`.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Card",
    "Metadata": {}
  }
}
```

###### Swish

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Swish",
    "Metadata": {}
  }
}
```

###### Invoice

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Invoice",
    "Metadata": {}
  }
}
```

###### Instalment

Instalment comes in three flavours, Default, Full KYC and Interest Free.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Instalment",
    "Metadata": {}
  }
}
```

##### Cancelled

Orders can be cancelled.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Cancelled",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CancelledDate": "2024-04-24T16:45:52.54Z",
    "Reason": "Timeout | UserCancelled | MerchantCancelled",
    "Metadata": {}
  }
}
```

##### Refunded

An order can be refunded and when successful this callback is sent.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "merchantId": "c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b",
  "status": "Refunded",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "RefundedDate": "2024-04-24T16:45:52.54Z",
    "Metadata": {}
  }
}
```
