using backend.Enums;

namespace backend.Dtos;

public sealed record TradeHistoryItem(
    int Id,
    string Symbol,
    string CompanyName,
    TradeType Type,
    int Quantity,
    decimal Price,
    decimal Total,
    decimal? PortfolioValueAfter,
    DateTime CreatedAt);
