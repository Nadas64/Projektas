using backend.Dtos;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class WatchlistController(WatchlistService watchlistService) : ControllerBase
{
    /// <summary>
    /// returns the user's watchlist ordered by symbol
    /// </summary>
    /// <response code="200">the watchlist</response>
    [HttpGet("watchlist")]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemResponse>>> GetWatchlist(
        CancellationToken cancellationToken)
    {
        var username = User.GetUsername();
        return Ok(await watchlistService.GetWatchlistAsync(username, cancellationToken));
    }

    /// <summary>
    /// adds a stock to the watchlist
    /// </summary>
    /// <param name="symbol">stock ticker</param>
    /// <response code="200">stock added, the updated watchlist is returned</response>
    /// <response code="400">error: "A stock symbol is required."</response>
    /// <response code="404">error: "Stock '{symbol}' was not found."</response>
    /// <response code="409">error: "This stock is already in the watchlist."</response>
    [HttpPost("watchlist/{symbol}")]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemResponse>>> AddToWatchlist(
        string symbol,
        CancellationToken cancellationToken)
    {
        var username = User.GetUsername();
        return Ok(await watchlistService.AddAsync(username, symbol, cancellationToken));
    }

    /// <summary>
    /// removes a stock from the watchlist
    /// </summary>
    /// <param name="symbol">stock ticker</param>
    /// <response code="200">stock removed, the updated watchlist is returned</response>
    /// <response code="400">error: "A stock symbol is required."</response>
    /// <response code="404">error: "This stock is not in the watchlist."</response>
    [HttpDelete("watchlist/{symbol}")]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemResponse>>> RemoveFromWatchlist(
        string symbol,
        CancellationToken cancellationToken)
    {
        var username = User.GetUsername();
        return Ok(await watchlistService.RemoveAsync(username, symbol, cancellationToken));
    }
}
