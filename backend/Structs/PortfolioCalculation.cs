namespace backend.Structs;

public struct PortfolioCalculation
{
    public decimal HoldingsValue { get; set; }
    public decimal TodaysGain { get; set; }
    public decimal TotalCost { get; set; }

    public decimal TotalBalance { get; set; }
    public decimal TotalGains { get; set; }
}