namespace backend.Models;

public class Holding
{
    public int Id { get; set; }

    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;

    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal AverageBuyPrice { get; set; }
}