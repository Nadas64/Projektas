namespace backend.Dtos;

public sealed record PortfolioResponse(
    decimal TotalBalance,
    decimal BuyingPower,
    decimal TodaysGain,
    decimal TodaysGainPercent,
    decimal TotalGains,
    decimal TotalGainsPercent,
    IReadOnlyList<HoldingResponse> Holdings);
