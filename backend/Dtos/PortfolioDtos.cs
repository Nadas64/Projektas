using backend.Models;

namespace backend.Dtos;

public sealed record TradeRequest(string Symbol, TradeType Type, int Quantity);
public sealed record HoldingResponse(string Symbol, int Quantity);
public sealed record PortfolioResponse(decimal Cash, IReadOnlyList<HoldingResponse> Holdings);