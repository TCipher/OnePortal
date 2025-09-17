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

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, fullName ?? email),
            new Claim("global_role", globalRoleCode ?? GlobalRoleCodes.User),
            // also include Role claim for built-in checks
            new Claim(ClaimTypes.Role, globalRoleCode ?? GlobalRoleCodes.User)
        };

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
