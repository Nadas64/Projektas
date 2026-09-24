namespace backend.Models;

public class Portfolio
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Cash { get; set; } = 100000;

    public ICollection<Holding> Holdings { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}