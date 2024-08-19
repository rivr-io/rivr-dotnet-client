using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rivr.Extensions;

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