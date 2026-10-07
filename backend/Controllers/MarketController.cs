using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class MarketController(FinnhubService finnhub) : ControllerBase
{
    /// <summary>
    /// returns the quote: current price, previous close, time
    /// </summary>
    /// <param name="symbol">stock ticker</param>
    [HttpGet("quote/{symbol}")]
    public async Task<ActionResult<StockQuote?>> GetQuote(
        string symbol,
        CancellationToken cancellationToken)
    {
        return Ok(await finnhub.GetQuoteAsync(symbol, cancellationToken));
    }

    /// <summary>
    /// searches US-limited stocks by name or symbol
    /// </summary>
    /// <param name="query">company name or symbol</param>
    /// <response code="200">list of matching stocks</response>
    /// <response code="400">error: "The query field is required."</response>
    [HttpGet("symbol/search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        return Ok(await finnhub.SearchSymbolsAsync(query));
    }
}