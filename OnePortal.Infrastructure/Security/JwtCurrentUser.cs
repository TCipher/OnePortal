using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OnePortal.Application.Abstractions;

namespace OnePortal.Infrastructure.Security;

public class JwtCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public JwtCurrentUser(IHttpContextAccessor http) => _http = http;

    public int? UserId
    {
        get
        {
            var user = _http.HttpContext?.User;
            var idStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user?.FindFirst("sub")?.Value;
            return int.TryParse(idStr, out var id) ? id : null;
        }
    }

    public bool IsInGlobalRole(string roleCode)
    {
        var user = _http.HttpContext?.User;
        var claim = user?.FindFirst("global_role")?.Value
                    ?? user?.FindFirst(ClaimTypes.Role)?.Value;
        return claim is not null &&
               string.Equals(claim, roleCode, StringComparison.OrdinalIgnoreCase);
    }
}
