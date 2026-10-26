using backend.Models;
using backend.Enums;

namespace backend.Dtos;

public sealed record TradeRequest(string Symbol, TradeType Type, int Quantity);
