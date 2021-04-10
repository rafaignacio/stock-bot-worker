using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using finance_bot.Worker.Services;

namespace finance_bot.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddJsonFile("appsettings.json");
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddHostedService<Worker>();
                    services.AddHttpClient<IStockService, StockService>();
                });
    }
}
