using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Rivr.Samples.CallbackHandler.Test;

public class TestableWebApplication<TWeb>(
    Action<IServiceCollection> configure
) :
    WebApplicationFactory<TWeb> where TWeb : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(configure);

        base.ConfigureWebHost(builder);
    }

    public static HttpClient CreateClient(Action<IServiceCollection> configure, WebApplicationFactoryClientOptions? options = default)
    {
        void AmplifiedConfiguration(IServiceCollection services)
        {
            configure(services);
        }

        var factory = new TestableWebApplication<TWeb>(AmplifiedConfiguration);

        options ??= new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        };

        var client = factory.CreateClient(options);
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Test", "1.0"));

        return client;
    }
}