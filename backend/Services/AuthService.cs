using backend.Data.Repositories;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Services;

public class AuthService(IUnitOfWork uow, IUserRepository users, IPasswordHasher<User> passwordHasher)
{
    public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var username = request.Username?.Trim() ?? "";
        var password = request.Password ?? "";

        if (username.Length < 3)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Username must be at least 3 characters.");
        }

        if (!IsStrongPassword(password))
        {
            throw new AppException(StatusCodes.Status400BadRequest,
                "Password must be 8-128 characters and contain an uppercase letter, a lowercase letter and a digit.");
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

        return user;
    }

    private static bool IsStrongPassword(string password)
        => password.Length is >= 8 and <= 128
           && password.Any(char.IsUpper)
           && password.Any(char.IsLower)
           && password.Any(char.IsDigit);

    public async Task<User> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var username = request.Username?.Trim() ?? "";
        var password = request.Password ?? "";

        var user = await users.GetByUsernameAsync(username, ct);

        // Same message for unknown user and wrong password, so usernames can't be probed.
        var result = user is null
            ? PasswordVerificationResult.Failed
            : passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Invalid username or password.");
        }

        return user!;
    }
}
