using Microsoft.Extensions.Hosting;
using Rivr;

public class MyService(IClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var health = await client.GetHealthAsync();
        Console.WriteLine("Result: " + health.Message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}