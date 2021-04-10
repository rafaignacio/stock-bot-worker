using System.Threading.Tasks;

namespace finance_bot.Worker.Services
{
    public interface IStockService
    {
        Task<string> GetStock(string stock_name);
    }
}