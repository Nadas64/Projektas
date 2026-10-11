using backend.Dtos;

namespace backend.Gateways;

public interface IFinnhubGateway
{
  Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken);
  Task<decimal?> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken);
  Task<string?> GetCompanyNameAsync(string symbol, CancellationToken cancellationToken);
  Task<bool> IsMarketOpenAsync(CancellationToken cancellationToken);
  Task<SymbolSearchResponse?> SearchSymbolsAsync(string query);
}