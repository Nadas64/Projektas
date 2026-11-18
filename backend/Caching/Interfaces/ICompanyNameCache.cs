namespace backend.Caching.Interfaces;

public interface ICompanyNameCache
{
    bool TryGet(string symbol, out string? companyName);

    bool TryAdd(string symbol, string companyName);
}
