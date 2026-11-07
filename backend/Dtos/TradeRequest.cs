using backend.Enums;
using backend.Models;

namespace backend.Dtos;

public sealed record TradeRequest(string Symbol, TradeType Type, int Quantity);
