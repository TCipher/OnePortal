using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Security
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly OnePortalDbContext _db;
        public RefreshTokenService(OnePortalDbContext db) => _db = db;

        public async Task<(string plaintext, string hash, DateTime expiresUtc)> IssueAsync(int userId, string? ip, string? ua, CancellationToken ct)
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(bytes);
            var hash = Hash(token);
            var exp = DateTime.UtcNow.AddMinutes(20);

            _db.RefreshTokens.Add(new RefreshToken { UserId = userId, TokenHash = hash, ExpiresUtc = exp, IpAddress = ip, DeviceInfo = ua });
            await _db.SaveChangesAsync(ct);
            return (token, hash, exp);
        }

        public async Task<(bool ok, int userId, string newToken, string newHash, DateTime newExp)> RotateAsync(string plaintext, CancellationToken ct)
        {
            var hash = Hash(plaintext);
            var current = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash && r.ConsumedUtc == null, ct);
            if (current is null || current.ExpiresUtc < DateTime.UtcNow) return (false, 0, "", "", DateTime.MinValue);

            current.ConsumedUtc = DateTime.UtcNow;

            // issue new
            var (token, newHash, exp) = await IssueAsync(current.UserId, current.IpAddress, current.DeviceInfo, ct);
            current.ReplacedByTokenHash = newHash;

            await _db.SaveChangesAsync(ct);
            return (true, current.UserId, token, newHash, exp);
        }

        public async Task RevokeByHashAsync(string hash, CancellationToken ct)
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
            if (rt is null) return;
            rt.ExpiresUtc = DateTime.UtcNow;
            rt.ConsumedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        private static string Hash(string token)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token)));
        }
    }
}
