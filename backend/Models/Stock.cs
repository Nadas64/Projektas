namespace backend.Models;

public class Stock
{
    public int Id { get; set; }

    public string Symbol { get; set; } = "";
    public string CompanyName { get; set; } = "";

    public ICollection<Holding> Holdings { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}