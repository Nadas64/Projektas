using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class PortfolioController(PortfolioService portfolioService) : ControllerBase
{
    [HttpGet("portfolio")]
    public async Task<ActionResult<IReadOnlyList<PortfolioHistoryPoint>>> GetHistory(CancellationToken cancellationToken)
    {
        return Ok(await portfolioService.GetPortfolioAsync(cancellationToken));
    }

    [HttpPost("trade")]
    public async Task<ActionResult<PortfolioResponse>> Trade(
        [FromBody] TradeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await portfolioService.TradeAsync(request, cancellationToken));
    }
}