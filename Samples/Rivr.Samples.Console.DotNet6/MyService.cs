using Microsoft.Extensions.Hosting;
using Rivr.Core;

namespace Rivr.Samples.Console.DotNet6;

public class MyService(IClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var health = await client.GetHealthAsync();
        System.Console.WriteLine("Result: " + health.Message);

        var healthSecure = await client
            .AsPlatform()
            .GetHealthSecureAsync();
        System.Console.WriteLine("Result: " + healthSecure.Message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}