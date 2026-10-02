using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class MarketController(FinnhubService finnhub) : ControllerBase
{
    /// <summary>
    /// returns the raw quote: current price (c), previous close (pc), time (t)
    /// </summary>
    /// <param name="symbol">stock ticker</param>
    [HttpGet("quote/{symbol}")]
    public async Task<IActionResult> GetQuote(string symbol)
        => Content(await finnhub.GetQuoteJsonAsync(symbol), "application/json");

    /// <summary>
    /// searches US-limited stocks by name
    /// </summary>
    /// <param name="query">company name or symbol</param>
    /// <response code="200">list of matching stocks</response>
    /// <response code="400">error: "The query field is required."</response>
    [HttpGet("symbol/search")]
    public async Task<IActionResult> Search([FromQuery] string query)
        => Ok(await finnhub.SearchSymbolsAsync(query));
}