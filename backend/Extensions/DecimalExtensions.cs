namespace backend.Extensions;

public static class DecimalExtensions
{
    /// <summary>Returns what percentage <paramref name="part"/> is of <paramref name="whole"/>.</summary>
    public static decimal PercentOf(this decimal part, decimal whole)
    {
        return whole == 0 ? 0 : Math.Round(part / whole * 100, 2);
    }
}
