using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class PortfolioController(PortfolioService portfolioService) : ControllerBase
{
    /// <summary>
    /// returns info about the portfolio: cash, holdings (with current prices), today's gains, total gains
    /// </summary>
    /// <response code="200">the portfolio with current data</response>
    [HttpGet("portfolio")]
    public async Task<ActionResult<PortfolioResponse>> GetPortfolio(CancellationToken cancellationToken)
    {
        return Ok(await portfolioService.GetPortfolioAsync(cancellationToken));
    }

    /// <summary>
    /// buys/sells shares at current market price
    /// </summary>
    /// <param name="request">symbol, trade type, quantity</param>
    /// <response code="200">trade successful, the updated portfolio is returned</response>
    /// <response code="400">
    /// errors: "Quantity must be greater than zero.", "A stock symbol is required.",
    /// "Insufficient cash for this purchase.", "Not enough shares are available to sell.",
    /// or a validation error for an invalid request body.
    /// </response>
    /// <response code="404">errors: "Portfolio record was not found.", "Stock record for this holding was not found."</response>
    /// <response code="502">error: "Could not retrieve a current price for this stock."</response>
    [HttpPost("trade")]
    public async Task<ActionResult<PortfolioResponse>> Trade(
        [FromBody] TradeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await portfolioService.TradeAsync(request, cancellationToken));
    }

    /// <summary>
    /// returns portfolio value history rebuilt from transactions (on each request)
    /// </summary>
    /// <param name="query">stock ticker</param>
    /// <response code="200">history points ordered by date</response>
    /// <response code="400">error: "The query field is required."</response>
    [HttpGet("portfolio/history")]
    public async Task<ActionResult<IReadOnlyList<PortfolioHistoryPoint>>> GetHistory(
        CancellationToken cancellationToken)
    {
        return Ok(await portfolioService.GetHistoryAsync(cancellationToken));
    }
}