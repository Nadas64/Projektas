namespace backend.Dtos;

public sealed record StockQuote(decimal Current, decimal PreviousClose, DateTime UpdatedAt);
