using System.Security.Claims;

namespace backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUsername(this ClaimsPrincipal user) =>
        user.Identity?.Name
        ?? throw new InvalidOperationException("Name claim missing - is [Authorize] applied?");
}

