using backend.Enums;

namespace backend.Dtos;

public sealed record LeaderboardItem(
    int Rank,
    string User,
    decimal AccountValue,
    decimal TotalReturn,
    decimal ProfitLoss);
