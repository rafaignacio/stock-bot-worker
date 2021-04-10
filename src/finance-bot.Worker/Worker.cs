using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using finance_bot.Worker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace finance_bot.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IStockService _stockService;

        public Worker(ILogger<Worker> logger, IConfiguration config, IStockService stockService)
        {
            _logger = logger;
            _config = config;
            _stockService = stockService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_config.GetConnectionString("rabbitMQ")) };
            factory.DispatchConsumersAsync = true;

            using var conn = factory.CreateConnection();
            using var channel = conn.CreateModel();


            var consumer = new AsyncEventingBasicConsumer(channel);

            channel.QueueDeclare("stock-income", false, false, false, null);
            channel.QueueDeclare("stock-results", false, false, false, null);

            consumer.Received += async (ch, msg) =>
            {
                var stock_name = Encoding.UTF8.GetString(msg.Body.ToArray());
                _logger.LogInformation("received stock name {stock}", stock_name);

                var stock = await _stockService.GetStock(stock_name);

                if (!string.IsNullOrWhiteSpace(stock))
                {
                    var props = channel.CreateBasicProperties();

                    props.ContentType = "text/plain";
                    props.DeliveryMode = 2;

                    channel.BasicPublish("", "stock-results", props, Encoding.UTF8.GetBytes(stock));
                }

                _logger.LogInformation("message received: {msg}", stock_name);
            };

            channel.BasicConsume("stock-income", true, consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
