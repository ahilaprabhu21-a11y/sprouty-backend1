using System.Security.Claims;

namespace Sprouty.Api.Helpers;

public static class ClaimsExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue("uid")
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var x) ? x : null;
    }

    public static int RequireUserId(this ClaimsPrincipal user)
    {
        return user.GetUserId() ?? throw new UnauthorizedAccessException("Token missing user id");
    }
}
