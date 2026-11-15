using System.Collections.Concurrent;

namespace backend.Caching;

/// <summary>
/// Process-wide cache of company names, shared by all request threads.
/// Company names are effectively immutable, so entries live for the lifetime
/// of the application and are not expired.
/// </summary>
public sealed class CompanyNameCache
{
    private readonly ConcurrentDictionary<string, string> _companyNames =
        new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string symbol, out string? companyName)
        => _companyNames.TryGetValue(symbol, out companyName);

    public bool TryAdd(string symbol, string companyName)
        => _companyNames.TryAdd(symbol, companyName);
}
