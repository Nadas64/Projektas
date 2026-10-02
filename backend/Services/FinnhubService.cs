using System.Text.Json;
using backend.Dtos;

namespace backend.Services;

public class FinnhubService(HttpClient http)
{
    public async Task<StockQuote?> GetQuoteAsync(
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
            var root = document.RootElement;
            if (root.TryGetProperty("c", out var currentElement)
                && currentElement.ValueKind == JsonValueKind.Number
                && currentElement.TryGetDecimal(out var current)
                && current > 0)
            {
                var previousClose =
                    root.TryGetProperty("pc", out var previousCloseElement)
                    && previousCloseElement.ValueKind == JsonValueKind.Number
                    && previousCloseElement.TryGetDecimal(out var pc)
                    && pc > 0
                        ? pc
                        : current;

                var updatedAt =
                    root.TryGetProperty("t", out var timeElement)
                    && timeElement.ValueKind == JsonValueKind.Number
                    && timeElement.TryGetInt64(out var unixSeconds)
                        ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime
                        : DateTime.UtcNow;

                return new StockQuote(current, previousClose, updatedAt);
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

    public async Task<decimal?> GetCurrentPriceAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        return (await GetQuoteAsync(symbol, cancellationToken))?.Current;
    }

    public async Task<string?> GetCompanyNameAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync($"stock/profile2?symbol={Uri.EscapeDataString(symbol)}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("name", out var name)
                && name.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(name.GetString()))
            {
                return name.GetString();
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