using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnePortal.Application.Abstractions;

namespace OnePortal.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(int userId, string email, string? fullName, string globalRoleCode)
    {
        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build the list of JWT claims.  In addition to the subject (sub), email,
        // name identifier, and full name, we also include a "global_role" claim
        // for application authorisation and a standard Role claim for the ASP.NET
        // security pipeline.  A given‑name claim is also included so clients
        // (such as the Blazor dashboard) can easily display just the user's
        // first name without having to parse the full name.  If a full name is
        // supplied, we derive the first name by taking the first token before
        // any spaces; otherwise we fall back to the email address.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, fullName ?? email),
            new Claim("global_role", globalRoleCode ?? GlobalRoleCodes.User),
            // also include Role claim for built‑in checks
            new Claim(ClaimTypes.Role, globalRoleCode ?? GlobalRoleCodes.User)
        };

        // Add a given name claim if we can derive one.  This allows the client
        // to obtain the user's first name directly from the token.  We avoid
        // making assumptions about the token format by checking for whitespace.
        var firstName = fullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            claims.Add(new Claim(ClaimTypes.GivenName, firstName));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),   // adjust as needed
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
