using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace finance_bot.Worker.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public StockService(ILogger<StockService> logger, HttpClient client)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<string> GetStock(string stock_name)
        {
            _logger.LogInformation("initiating request");

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://stooq.com/q/l/?s={stock_name}&f=sd2t2ohlcv&h&e=csv");
            var response = await _client.SendAsync(request);

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }
    }
}