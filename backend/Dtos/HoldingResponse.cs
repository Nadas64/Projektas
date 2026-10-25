namespace backend.Dtos;

public sealed record HoldingResponse(
    string Symbol,
    string CompanyName,
    int Quantity,
    decimal AverageBuyPrice,
    decimal CurrentValue,
    decimal GainPercent,
    decimal GainDollars);
