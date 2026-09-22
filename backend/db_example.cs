using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly AppDbContext _db;

    public StocksController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /api/stocks
    [HttpGet]
    public async Task<ActionResult<List<Stock>>> GetAll()
    {
        var stocks = await _db.Stocks.ToListAsync();

        return Ok(stocks);
    }

    // GET: /api/stocks/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Stock>> GetById(int id)
    {
        var stock = await _db.Stocks.FindAsync(id);

        if (stock == null)
        {
            return NotFound();
        }

        return Ok(stock);
    }

    // GET: /api/stocks/{symbol}
    [HttpGet("symbol/{symbol}")]
    public async Task<ActionResult<Stock>> GetBySymbol(string symbol)
    {
        var stock = await _db.Stocks
            .FirstOrDefaultAsync(s => s.Symbol == symbol);

        if (stock == null)
        {
            return NotFound();
        }

        return Ok(stock);
    }

    // POST: /api/stocks
    [HttpPost]
    public async Task<ActionResult<Stock>> Create(Stock stock)
    {
        _db.Stocks.Add(stock);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = stock.Id },
            stock
        );
    }

    // PUT: /api/stocks/1
//  body example
//  {
//     "Symbol" : "NVDA",
//     "CompanyName": "nvidia"
//  }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Stock updatedStock)
    {
        var stock = await _db.Stocks.FindAsync(id);

        if (stock == null)
        {
            return NotFound();
        }

        stock.Symbol = updatedStock.Symbol;
        stock.CompanyName = updatedStock.CompanyName;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: /api/stocks/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var stock = await _db.Stocks.FindAsync(id);

        if (stock == null)
        {
            return NotFound();
        }

        _db.Stocks.Remove(stock);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}