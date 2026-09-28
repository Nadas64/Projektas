namespace backend.Dtos;

public sealed record TradeRequest(string Symbol, string Type, int Quantity);
public sealed record HoldingResponse(string Symbol, int Quantity);
public sealed record PortfolioResponse(decimal Cash, IReadOnlyList<HoldingResponse> Holdings);