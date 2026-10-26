namespace backend.Exceptions;

public sealed class InsufficientCashException()
    : AppException(StatusCodes.Status400BadRequest, "Insufficient cash for this purchase.");
