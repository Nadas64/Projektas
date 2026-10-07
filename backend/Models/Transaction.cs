namespace backend.Models;

public class Transaction
{
    public int Id { get; set; }

    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;

    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public TradeType Type { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal? PortfolioValueAfter { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}