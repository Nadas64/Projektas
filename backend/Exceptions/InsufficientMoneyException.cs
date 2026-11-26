namespace backend.Exceptions;

public sealed class InsufficientMoneyException()
    : AppException(StatusCodes.Status400BadRequest, "Insufficient money for this purchase.");
