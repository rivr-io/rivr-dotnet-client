# Rivr .NET Client

[![nuget](https://img.shields.io/nuget/v/rivr.svg)](https://www.nuget.org/packages/rivr/) [![nuget](https://img.shields.io/nuget/dt/rivr.svg)](https://www.nuget.org/packages/rivr/) [![Build and tests](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml/badge.svg)](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml)

## Overview

This is an API Client library for Rivr Order API. This library enables an application to send orders for processing.

## Compability

This library is built with support with .NET Standard 2.1 which provides compability with .NET and .NET Core versions 3.0, 3.1, 5.0, 6.0, 7.0, 8.0 and 9.0. If you require support for .NET Framework, please feel free to reach out to support@rivr.io.

## Installation

This libary is published on Nuget. Use your favourite package manager to add it to your project.

```
dotnet add package Rivr
```

## Security

The industry-standard [OAuth2](https://oauth.net/2/) security protocol is used for authorization.

### Credentials

You will need a `client_id` and `client_secret`. Reach out to support@rivr.io and we will setup your application and provide you with the credentials you need.

### Security contexts

As a consumer of this library you will be able to perform actions as a platform or on behalf of the merchants you manage. Using this library, we have taken care of the authentication for you.

#### Platform level

The platform level authentication utilizes a simplified version of OAuth2 and uses `grant_type = client_credentials`, which uses `client_id` and `client_secret` to fetch an `access_token`.

#### Merchant level

The merchant level authentication is a variant of OAuth2 which allows the system to act on behalf of a merchant. It is very similar to the system level authentication, but uses our own `grant_type = merchant_credentials`. With this, the `client_id` and the `client_secret` along with a `merchant_id` are used to fetch an `access_token` that includes the `merchant_id`. This allows for the system to perform actions on behalf of the authorized merchant.

## Using the client

The following section describes how to use this library.

### Creating an instance of the client

```C#
var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
var memoryCache = new MemoryCache(new MemoryCacheOptions());

var client = new Client(config, memoryCache);
```

#### Check API health (unauthenticated)

Check that the API is online and operational using the `GetHealthAsync` method.

```C#
var result = await client.GetHealthAsync();
// result is "OK"
```

### Platform operations

These operations are performed as a platform.

#### Check API health (using authentication)

Check that the API is online and operational using the `GetHealthSecureAsync` method. This is also a way to test that the authentication works as intended.

```C#
var result = await client
    .AsPlatform()
    .GetHealthSecureAsync();
// result is "OK"
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
// result is "OK"
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
    .GetOrderAsync(order);
// result contains an instance of an Order object
```
