using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IRefreshTokenService
    {
        Task<(string plaintext, string hash, DateTime expiresUtc)> IssueAsync(int userId, string? ip, string? ua, CancellationToken ct);
        Task<(bool ok, int userId, string newToken, string newHash, DateTime newExp)> RotateAsync(string plaintext, CancellationToken ct);
        Task RevokeByHashAsync(string hash, CancellationToken ct);
    }
}
