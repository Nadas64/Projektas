using System.Text.Json.Serialization;

namespace backend.Dtos;

public sealed record SymbolSearchResponse(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("result")] IReadOnlyList<SymbolSearchResult> Result);
