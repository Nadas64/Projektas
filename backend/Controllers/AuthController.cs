using backend.Dtos;
using backend.Extensions;
using backend.Models;
using backend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var user = await authService.RegisterAsync(request, cancellationToken);
        await SignInAsync(user);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await authService.LoginAsync(request, cancellationToken);
        await SignInAsync(user);
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok();
    }

    [HttpGet("me")]
    public ActionResult<UserResponse> Me()
    {
        string? username;
        try
        {
            username = User.GetUsername();
        }
        catch (InvalidOperationException)
        {
            username = null;
        }

        return Ok(new UserResponse(username));
    }

    private Task SignInAsync(User user)
    {
        var identity = new ClaimsIdentity(
            [ new Claim(ClaimTypes.Name, user.Username) ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return HttpContext.SignInAsync(new ClaimsPrincipal(identity));
    }
}
