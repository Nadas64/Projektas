using backend.Data.Repositories.Interfaces;
using backend.Dtos;
using backend.Exceptions;
using backend.Extensions;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Services;

public class AuthService(IUnitOfWork uow, IPasswordHasher<User> passwordHasher)
{
    public async Task<User> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.TrimOrEmpty();
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

        if (await uow.Users.UsernameExistsAsync(username, cancellationToken))
        {
            throw new AppException(StatusCodes.Status409Conflict, "Username is already taken.");
        }

        var user = new User
        {
            Username = username,
            Portfolio = new Portfolio { Cash = PortfolioService.StartingCash }
        };
        user.HashedPassword = passwordHasher.HashPassword(user, password);

        await uow.Users.AddAsync(user, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return user;
    }

    private static bool IsStrongPassword(string password)
        => password.Length is >= 8 and <= 128
           && password.Any(char.IsUpper)
           && password.Any(char.IsLower)
           && password.Any(char.IsDigit);

    public async Task<User> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.TrimOrEmpty();
        var password = request.Password ?? "";

        var user = await uow.Users.GetByUsernameAsync(username, cancellationToken);

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
