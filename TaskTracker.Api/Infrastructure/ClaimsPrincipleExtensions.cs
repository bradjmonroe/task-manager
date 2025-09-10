using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskTracker.Api.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(id, out var gid) ? gid : null;
    }
}
