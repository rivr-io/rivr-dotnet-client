using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rivr.Core;
using Rivr.Core.Models.Orders;

namespace Rivr.Samples.Console.DependencyInjection;

public class MyService(IClient client, ILogger<MyService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking API health (unauthenticated)");
        var health = await client.GetHealthAsync();
        logger.LogInformation("Result: " + health.Message);


        logger.LogInformation("Checking API health (authenticated)");
        var healthAsPlatform = await client
            .AsPlatform()
            .GetHealthSecureAsync();
        logger.LogInformation("Result: " + healthAsPlatform.Message);


        logger.LogInformation("Getting merchants");
        var merchantsResponse = await client
            .AsPlatform()
            .GetMerchantsAsync();


        logger.LogInformation("Merchants:");
        foreach (var merchant in merchantsResponse.Merchants)
        {
            logger.LogInformation("\t- " + merchant.Name);
        }


        var merchantId = merchantsResponse.Merchants.First().Id;

        logger.LogInformation("Checking API health (on behalf of merchant)");
        var healthOnBehalfOfMerchant = await client
            .OnBehalfOfMerchant(merchantId)
            .GetHealthSecureAsync();
        logger.LogInformation("Result: " + healthOnBehalfOfMerchant.Message);


        logger.LogInformation("Checking API health (on behalf of merchant)");
        var devices = await client
            .OnBehalfOfMerchant(merchantId)
            .GetDevicesAsync();

        foreach (var device in devices)
        {
            logger.LogInformation("Device: " + device.Name);
            logger.LogInformation("\t- Id: " + device.Id);
            logger.LogInformation("\t- DeviceUniqueId: " + device.DeviceUniqueId);
            logger.LogInformation("\t- IsOnline: " + device.IsOnline);
        }


        logger.LogInformation("Creating an order");
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
        logger.LogInformation("Order created with ID: " + orderResult.Id);

        logger.LogInformation("Waiting 2 seconds before getting order");
        Thread.Sleep(2000);

        logger.LogInformation("Getting order");
        var orderResponse = await client
            .OnBehalfOfMerchant(merchantId)
            .GetOrderAsync(orderResult.Id);
        logger.LogInformation("Order ID: " + orderResponse.Id);


        logger.LogInformation("Refund order");
        await client
            .OnBehalfOfMerchant(merchantId)
            .RefundAsync(orderResult.Id);
        logger.LogInformation("Order refunded");


        logger.LogInformation("Press any key to exit");
        System.Console.ReadKey(true);
        Environment.Exit(0);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}