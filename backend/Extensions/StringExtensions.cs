namespace backend.Extensions;

public static class StringExtensions
{
    /// <summary>Trims whitespace from the string, or returns an empty string if it is null.</summary>
    public static string TrimOrEmpty(this string? value)
        => value?.Trim() ?? "";
}
