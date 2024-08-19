# Rivr .NET Client

[![nuget](https://img.shields.io/nuget/v/rivr.svg)](https://www.nuget.org/packages/rivr/) [![nuget](https://img.shields.io/nuget/dt/rivr.svg)](https://www.nuget.org/packages/rivr/) [![Build and tests](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml/badge.svg)](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml)

## Overview

This is an API Client library for Rivr Order API. This library enables an application to send orders for processing.

### Compability

This library is built with support with [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) which provides the following compability:

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

As a consumer of this library you will be able to perform actions as a platform or on behalf of the merchants you manage. Using this library, we have taken care of the authentication for you.

##### Platform level

The platform level authentication utilizes a simplified version of OAuth2 and uses `grant_type = client_credentials`, which uses `client_id` and `client_secret` to fetch an `access_token`.

##### Merchant level

The merchant level authentication is a variant of OAuth2 which allows the system to act on behalf of a merchant. It is very similar to the system level authentication, but uses our own `grant_type = merchant_credentials`. With this, the `client_id` and the `client_secret` along with a `merchant_id` are used to fetch an `access_token` that includes the `merchant_id`. This allows for the system to perform actions on behalf of the authorized merchant.

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

                To run this sample, you need to provide your client ID and client secret.

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
    .OnBehalfOfMerchant(merchantId)
    .GetHealthSecureAsync();
// result.Message is "OK"
```

#### Fetch devices for a merchant

A merchant can be configured to have zero or more devices (checkout terminals). If a payment is to be sent to a specific device, the deviceId must be added to the order creation. Use this operation to fetch all the devices that the merchant has.

```C#
var devices = await client
    .OnBehalfOfMerchant(sdkMerchantId)
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
    .OnBehalfOfMerchant(merchantId)
    .CreateOrderAsync(order);
// result contains an instance of an Order object
```

#### Fetch an order

Using the `order.Id`, the order can be retrieved.

```C#
var merchantId = Guid.Parse("...");
var result = await client
    .OnBehalfOfMerchant(merchantId)
    .GetOrderAsync(order.Id);
// result contains an instance of an Order object
```

#### Refund or Cancel an order

And order can be either refunded or cancelled depending on the current status. If the status is `OrderStatus.Completed`, the order will be refunded. If the status is `OrderStatus.Created`, the order will be cancelled. The calling system does not need to know the current status of the order.

Note: There are certain payment methods that do not support refunds (some instalment products).

```C#
var merchantId = Guid.Parse("...");
await client
    .OnBehalfOfMerchant(merchantId)
    .RefundAsync(order.Id);
```

## Callbacks

The integration is based on the calling system being able to receive callbacks (webhooks). The callback contains the orderId and the `OrderStatus` describing the event that occurred. Depending on the event, there may be more information provided. The callback is delivered to the `callbackUrl` that is set when creating the order.

Example of a callback body:

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Created",
  "data": {
    "createdDate": "2024-08-19 14:58:21"
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

Note: If the Callback URL is empty or malformatted there will be no attempts at all.

#### Order

Here follows a detailed overview of the callbacks related to the Order entity.

##### Created

Immediately after an order is created a callback is issued.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Created"
}
```

##### Completed

A order is considered `Completed` when it has been paid. An Order can be paid with a different payment methods. The different variants of callbacks are listed here.

###### Card

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Card"
  }
}
```

###### Apple Pay / Google Pay

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "DigitalWallet"
  }
}
```

###### Swish

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Swish"
  }
}
```

###### Invoice

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "Invoice"
  }
}
```

###### Instalment

Instalment comes in three flavours, Default, Full KYC and Interest Free.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "InstalmentDefault"
  }
}
```

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "InstalmentFullKyc"
  }
}
```

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Completed",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CompletedDate": "2024-04-24T16:45:52.54Z",
    "PaymentMethod": "InstalmentInterestFree"
  }
}
```

##### Cancelled

Orders can be cancelled.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Cancelled",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "CancelledDate": "2024-04-24T16:45:52.54Z",
    "Reason": "Timeout | UserCancelled | MerchantCancelled"
  }
}
```

##### Refunded

An order can be refunded and when successful this callback is sent.

```json
{
  "id": "84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39",
  "type": "Order",
  "status": "Refunded",
  "data": {
    "CreatedDate": "2024-04-24T16:45:30.582Z",
    "RefundedDate": "2024-04-24T16:45:52.54Z"
  }
}
```
