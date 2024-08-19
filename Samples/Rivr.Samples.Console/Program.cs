// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Caching.Memory;
using Rivr;
using Rivr.Models.Orders;

Console.WriteLine("Testing Rivr API");
Console.WriteLine();


var config = new Config(clientId: "enter-client-id", clientSecret: "enter-client-secret")
{
    Environment = Rivr.Models.Environment.Test
};

var memoryCache = new MemoryCache(new MemoryCacheOptions());

var client = new Client(config, memoryCache);

Console.WriteLine("Created an instance of the client");
Console.WriteLine();


Console.WriteLine("Checking API health (unauthenticated)");
var health = await client.GetHealthAsync();
Console.WriteLine("Result: " + health.Message);
Console.WriteLine();


Console.WriteLine("Checking API health (authenticated)");
var healthAsPlatform = await client
    .AsPlatform()
    .GetHealthSecureAsync();
Console.WriteLine("Result: " + healthAsPlatform.Message);
Console.WriteLine();


Console.WriteLine("Getting merchants");
var merchantsResponse = await client
    .AsPlatform()
    .GetMerchantsAsync();


Console.WriteLine("Merchants:");
foreach (var merchant in merchantsResponse.Merchants)
{
    Console.WriteLine("\t- " + merchant.Name);
}

Console.WriteLine();

var merchantId = merchantsResponse.Merchants.First().Id;

Console.WriteLine("Checking API health (on behalf of merchant)");
var healthOnBehalfOfMerchant = await client
    .OnBehalfOfMerchant(merchantId)
    .GetHealthSecureAsync();
Console.WriteLine("Result: " + healthOnBehalfOfMerchant.Message);
Console.WriteLine();

Console.WriteLine("Checking API health (on behalf of merchant)");
var devices = await client
    .OnBehalfOfMerchant(merchantId)
    .GetDevicesAsync();
Console.WriteLine();
foreach (var device in devices)
{
    Console.WriteLine("Device: " + device.Name);
    Console.WriteLine("\t- Id: " + device.Id);
    Console.WriteLine("\t- DeviceUniqueId: " + device.DeviceUniqueId);
    Console.WriteLine("\t- IsOnline: " + device.IsOnline);
    Console.WriteLine();
}

Console.WriteLine();

Console.WriteLine("Creating an order");
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

var orderResult = await client
    .OnBehalfOfMerchant(merchantId)
    .CreateOrderAsync(order);
Console.WriteLine("Order created with ID: " + orderResult.Id);
Console.WriteLine();


Console.WriteLine("Getting order");
var orderResponse = await client
    .OnBehalfOfMerchant(merchantId)
    .GetOrderAsync(orderResult.Id);
Console.WriteLine("Order ID: " + orderResponse.Id);
Console.WriteLine();


Console.WriteLine("Refund order");
await client
    .OnBehalfOfMerchant(merchantId)
    .RefundAsync(orderResult.Id);
Console.WriteLine("Order refunded");
Console.WriteLine();


Console.WriteLine("Press any key to exit");
Console.ReadKey(true);

Environment.Exit(0);