using backend.Data.Repositories;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Services;

public class AuthService(IUnitOfWork uow, IUserRepository users, IPasswordHasher<User> passwordHasher)
{
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

        var user = new User
        {
            Username = username,
            Portfolio = new Portfolio { Cash = PortfolioService.StartingCash }
        };
        user.HashedPassword = passwordHasher.HashPassword(user, password);

        await users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
    }
}
