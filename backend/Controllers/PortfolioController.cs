using System.Text.Json;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class PortfolioController(
    AppDbContext db,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ControllerBase
{
    private const string DefaultUsername = "default";
    private const decimal StartingCash = 1000m;
    private readonly AppDbContext _db = db;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet("portfolio")]
    public async Task<ActionResult<PortfolioResponse>> GetPortfolio()
    {
        var portfolio = await GetOrCreateDefaultPortfolioAsync();
        return Ok(await CreatePortfolioResponseAsync(portfolio));
    }

    [HttpPost("trade")]
    public async Task<ActionResult<PortfolioResponse>> Trade(
        [FromBody] TradeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than zero.");
        }

        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var type = request.Type.Trim().ToUpperInvariant();
        if (symbol.Length == 0 || (type != "BUY" && type != "SELL"))
        {
            return BadRequest("A stock symbol and trade type BUY or SELL are required.");
        }

        var apiKey = _configuration["FINNHUB_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Stock price service is unavailable.");
        }

        var marketIsOpen = await IsMarketOpenAsync(apiKey, cancellationToken);
        if (marketIsOpen is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "Could not verify whether the market is open.");
        }

        if (!marketIsOpen.Value)
        {
            return Conflict("Trading is unavailable while the market is closed.");
        }

        var price = await GetCurrentPriceAsync(symbol, apiKey, cancellationToken);
        if (price is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "Could not retrieve a current price for this stock.");
        }

        // TODO: Remade logic after registration and users system creation
        // FIXME
        var defaultPortfolio = await GetOrCreateDefaultPortfolioAsync();
        await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var portfolioId = defaultPortfolio.Id;
        _db.Entry(defaultPortfolio).State = EntityState.Detached;
        var portfolio = await _db.Portfolios
            .FromSqlInterpolated($"SELECT * FROM \"Portfolios\" WHERE \"Id\" = {portfolioId} FOR UPDATE")
            .SingleAsync(cancellationToken);

        var holding = await _db.Holdings
            .SingleOrDefaultAsync(
                item => item.PortfolioId == portfolio.Id && item.Stock.Symbol == symbol,
                cancellationToken);
        var stock = await _db.Stocks.SingleOrDefaultAsync(
            item => item.Symbol == symbol,
            cancellationToken);
        var total = price.Value * request.Quantity;

        if (type == "BUY")
        {
            if (portfolio.Cash < total)
            {
                return BadRequest("Insufficient cash for this purchase.");
            }

            if (stock is null)
            {
                stock = new Stock { Symbol = symbol, CompanyName = symbol };
                _db.Stocks.Add(stock);
            }

            if (holding is null)
            {
                holding = new Holding
                {
                    PortfolioId = portfolio.Id,
                    Stock = stock,
                    Quantity = request.Quantity,
                    AverageBuyPrice = price.Value
                };
                _db.Holdings.Add(holding);
            }
            else
            {
                var updatedQuantity = holding.Quantity + request.Quantity;
                holding.AverageBuyPrice =
                    (holding.AverageBuyPrice * holding.Quantity + total) / updatedQuantity;
                holding.Quantity = updatedQuantity;
            }

            portfolio.Cash -= total;
        }
        else
        {
            if (holding is null || holding.Quantity < request.Quantity)
            {
                return BadRequest("Not enough shares are available to sell.");
            }

            if (stock is null)
            {
                return NotFound("Stock record for this holding was not found.");
            }

            holding.Quantity -= request.Quantity;
            if (holding.Quantity == 0)
            {
                _db.Holdings.Remove(holding);
            }

            portfolio.Cash += total;
        }

        _db.Transactions.Add(new Transaction
        {
            PortfolioId = portfolio.Id,
            Stock = stock,
            Type = type,
            Quantity = request.Quantity,
            Price = price.Value,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);

        return Ok(await CreatePortfolioResponseAsync(portfolio));
    }

    private async Task<Portfolio> GetOrCreateDefaultPortfolioAsync()
    {
        var user = await _db.Users
            .Include(item => item.Portfolio)
            .SingleOrDefaultAsync(item => item.Username == DefaultUsername);

        if (user is null)
        {
            user = new User
            {
                Username = DefaultUsername,
                Password = "",
                Portfolio = new Portfolio { Cash = StartingCash }
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        else if (user.Portfolio is null)
        {
            user.Portfolio = new Portfolio { Cash = StartingCash };
            await _db.SaveChangesAsync();
        }

        return user.Portfolio;
    }

    private async Task<PortfolioResponse> CreatePortfolioResponseAsync(Portfolio portfolio)
    {
        var holdings = await _db.Holdings
            .Where(item => item.PortfolioId == portfolio.Id)
            .OrderBy(item => item.Stock.Symbol)
            .Select(item => new HoldingResponse(item.Stock.Symbol, item.Quantity))
            .ToListAsync();

        return new PortfolioResponse(portfolio.Cash, holdings);
    }

    private async Task<decimal?> GetCurrentPriceAsync(
        string symbol,
        string apiKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://finnhub.io/api/v1/quote?symbol={Uri.EscapeDataString(symbol)}&token={Uri.EscapeDataString(apiKey)}";
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("c", out var currentPrice)
                && currentPrice.TryGetDecimal(out var price)
                && price > 0)
            {
                return price;
            }
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private async Task<bool?> IsMarketOpenAsync(string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://finnhub.io/api/v1/stock/market-status?exchange=US&token={Uri.EscapeDataString(apiKey)}";
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("isOpen", out var isOpen)
                && (isOpen.ValueKind == JsonValueKind.True || isOpen.ValueKind == JsonValueKind.False))
            {
                return isOpen.GetBoolean();
            }
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}

public sealed record TradeRequest(string Symbol, string Type, int Quantity);
public sealed record HoldingResponse(string Symbol, int Quantity);
public sealed record PortfolioResponse(decimal Cash, IReadOnlyList<HoldingResponse> Holdings);