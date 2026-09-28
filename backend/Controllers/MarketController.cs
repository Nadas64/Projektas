using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class MarketController(FinnhubService finnhub) : ControllerBase
{
  [HttpGet("quote/{symbol}")]
  public async Task<IActionResult> GetQuote(string symbol)
      => Content(await finnhub.GetQuoteJsonAsync(symbol), "application/json");

  [HttpGet("symbol/search")]
  public async Task<IActionResult> Search([FromQuery] string query)
      => Content(await finnhub.SearchSymbolsJsonAsync(query), "application/json");
}