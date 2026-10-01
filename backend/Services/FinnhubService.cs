using System.Text.Json;
using backend.Dtos;

namespace backend.Services;

public class FinnhubService(HttpClient http)
{
    public async Task<decimal?> GetCurrentPriceAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync($"quote?symbol={Uri.EscapeDataString(symbol)}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("c", out var currentPrice)
                && currentPrice.TryGetDecimal(out var price)
                && price > 0)
            {
                return price;
            }
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    public async Task<bool> IsMarketOpenAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync("stock/market-status?exchange=US", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("isOpen", out var isOpen)
                && (isOpen.ValueKind == JsonValueKind.True || isOpen.ValueKind == JsonValueKind.False))
            {
                return isOpen.GetBoolean();
            }
        }
        catch
        {
            return false;
        }
        return false;
    }

    public async Task<string> GetQuoteJsonAsync(string symbol)
    {
        var response = await http.GetAsync($"quote?symbol={symbol}");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<SymbolSearchResponse?> SearchSymbolsAsync(string query)
    {
        var response = await http.GetAsync($"search?q={query}&exchange=US");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SymbolSearchResponse>(json);
        if (result?.Result is null)
        {
            return result;
        }

        var deduped = result.Result
            .GroupBy(r => r.Symbol)
            .Select(g => g.MinBy(r => r.Description.Length)!)
            .ToList();

        return new SymbolSearchResponse(deduped.Count, deduped);
    }
}