using backend.Data.Repositories;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;

namespace backend.Services;

public class AuthService(IUnitOfWork uow, IUserRepository users)
{
    // TODO: hash passwords (currently stored as plain text)
    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var username = request.Username?.Trim() ?? "";
        var password = request.Password ?? "";

        if (username.Length < 3)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Username must be at least 3 characters.");
        }

        if (password.Length < 6)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Password must be at least 6 characters.");
        }

        if (await users.UsernameExistsAsync(username, ct))
        {
            throw new AppException(StatusCodes.Status409Conflict, "Username is already taken.");
        }

        await users.AddAsync(new User
        {
            Username = username,
            Password = password,
            Portfolio = new Portfolio { Cash = PortfolioService.StartingCash }
        }, ct);
        await uow.SaveChangesAsync(ct);
    }
}
