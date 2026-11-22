namespace backend.Models;

public class WatchlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
