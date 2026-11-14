using backend.Data.Repositories.Interfaces;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Backend.Tests.Services;

public class AuthServiceTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _uow.Users.Returns(_users);
        _service = new AuthService(_uow, _passwordHasher);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_SavesUserWithHashedPasswordAndStartingCash()
    {
        var user = await _service.RegisterAsync(new RegisterRequest(" alice ", "Secret123"), default);

        Assert.Equal("alice", user.Username);
        Assert.Equal(PortfolioService.StartingCash, user.Portfolio.Cash);
        Assert.Equal(PasswordVerificationResult.Success,
            _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, "Secret123"));
        await _users.Received().AddAsync(user, default);
        await _uow.Received().SaveChangesAsync(default);
    }

    [Theory]
    [InlineData("alice", "WrongPass1")]
    [InlineData("bob", "Bob'sSecret")]
    public async Task LoginAsync_WrongPasswordOrUnknownUser_ThrowsUnauthorized(string username, string password)
    {
        var alice = new User { Username = "alice" };
        alice.HashedPassword = _passwordHasher.HashPassword(alice, "Secret123");
        _users.GetByUsernameAsync("alice", default).Returns(alice);

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _service.LoginAsync(new LoginRequest(username, password), default));

        Assert.Equal(StatusCodes.Status401Unauthorized, ex.StatusCode);
    }
}
