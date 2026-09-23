namespace backend.Models;

public class Transaction
{
    public int Id { get; set; }

    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;

    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public string Type { get; set; } = ""; // BUY / SELL

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}