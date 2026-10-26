using System.Text.Json.Serialization;

namespace backend.Dtos;

public sealed record SymbolSearchResult(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("displaySymbol")] string DisplaySymbol,
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("type")] string Type);
