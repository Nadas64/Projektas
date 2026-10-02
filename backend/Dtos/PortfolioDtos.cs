using backend.Models;

namespace backend.Dtos;

public sealed record TradeRequest(string Symbol, TradeType Type, int Quantity);
public sealed record HoldingResponse(
    string Symbol,
    string CompanyName,
    int Quantity,
    decimal AverageBuyPrice,
    decimal CurrentValue,
    decimal GainPercent,
    decimal GainDollars);

public sealed record PortfolioResponse(
    decimal TotalBalance,
    decimal BuyingPower,
    decimal TodaysGain,
    decimal TodayGainPercent,
    decimal TotalGains,
    decimal TotalGainsPercent,
    IReadOnlyList<HoldingResponse> Holdings);

public sealed record PortfolioHistoryPoint(DateTime Date, decimal Value);