// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Caching.Memory;
using Rivr;

Console.WriteLine("Hello, World!");

var config = new Config(clientId: "clientId", clientSecret: "clientSecret");
var memoryCache = new MemoryCache(new MemoryCacheOptions());

var client = new Client(config, memoryCache);

Console.WriteLine("Created an instance of the client");
Console.WriteLine();


Console.WriteLine("Checking API health (unauthenticated)");
var health = await client.GetHealthAsync();
Console.WriteLine("Result: " + health.Message);
Console.WriteLine();


Console.WriteLine("Checking API health (unauthenticated)");
var healthAsPlatform = await client
    .AsPlatform()
    .GetHealthSecureAsync();
Console.WriteLine("Result: " + healthAsPlatform.Message);
Console.WriteLine();


Console.WriteLine("Getting merchants (unauthenticated)");
var merchants = await client
    .AsPlatform()
    .GetMerchantsAsync();


Console.WriteLine("Merchants:");
foreach (var merchant in merchants.Merchants)
{
    Console.WriteLine("\t- " + merchant.Name);
}

Console.WriteLine();