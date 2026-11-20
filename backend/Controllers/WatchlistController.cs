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
    [HttpGet("watchlist")]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemResponse>>> GetWatchlist(
        CancellationToken cancellationToken)
    {
        var username = User.GetUsername();
        return Ok(await watchlistService.GetWatchlistAsync(username, cancellationToken));
    }

    [HttpPost("watchlist/{symbol}")]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemResponse>>> AddToWatchlist(
        string symbol,
        CancellationToken cancellationToken)
    {
        var username = User.GetUsername();
        return Ok(await watchlistService.AddAsync(username, symbol, cancellationToken));
    }

    [HttpDelete("watchlist/{symbol}")]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemResponse>>> RemoveFromWatchlist(
        string symbol,
        CancellationToken cancellationToken)
    {
        var username = User.GetUsername();
        return Ok(await watchlistService.RemoveAsync(username, symbol, cancellationToken));
    }
}
