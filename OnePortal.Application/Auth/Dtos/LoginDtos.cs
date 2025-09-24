using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Auth.Dtos;

public record MfaStateDto
{
    public bool Required { get; init; }
    public string? Method { get; init; }       // EmailOtp
    public string? ChallengeId { get; init; }
}

public record LoginResultDto
{
    public string? TokenType { get; init; }
    public string? AccessToken { get; init; }
    public int? ExpiresIn { get; init; }
    public string? RefreshToken { get; init; }
    public int? RefreshExpiresIn { get; init; }
    public MfaStateDto? Mfa { get; init; }
    public bool MustChangePassword { get; init; }

    public static LoginResultDto Issued(string access, int accessSec, string refresh, int refreshSec) =>
        new()
        {
            TokenType = "Bearer",
            AccessToken = access,
            ExpiresIn = accessSec,
            RefreshToken = refresh,
            RefreshExpiresIn = refreshSec,
            Mfa = new MfaStateDto { Required = false }
        };
}
