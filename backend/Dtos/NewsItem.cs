namespace backend.Dtos;

public sealed record NewsItem(string Headline, string Summary, string Source, string Url, long Datetime);