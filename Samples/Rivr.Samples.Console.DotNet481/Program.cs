using System;
using Rivr.Core.Models;
using Rivr.DotNet48;
using Environment = Rivr.Core.Models.Environment;

namespace Rivr.Samples.Console.DotNet481
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello");

            var client = new Client(
                new Config(
                    clientId: "client-id",
                    clientSecret: "client-secret",
                    environment: Environment.Test)
            );

            var health = client.GetHealthAsync().Result;

            System.Console.WriteLine(health.Message);

            var healthSecure = client
                .AsPlatform()
                .GetHealthSecureAsync()
                .Result;

            System.Console.WriteLine(healthSecure.Message);
        }
    }
}