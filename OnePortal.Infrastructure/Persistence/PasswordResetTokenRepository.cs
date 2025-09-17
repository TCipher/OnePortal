using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Persistence
{
    internal class PasswordResetTokenRepository : IPasswordResetTokenRepository
   {
        private readonly OnePortalDbContext _db;

        public PasswordResetTokenRepository(OnePortalDbContext db) => _db = db;

        public async Task<PasswordResetToken?> FindActiveByHashAsync(string tokenHash, CancellationToken ct)
        {
            // Track entity for update; ensure active (unused) – expiry checked by handler for clear flow
            return await _db.Set<PasswordResetToken>()
                .Where(t => t.TokenHash == tokenHash && t.UsedUtc == null)
                .OrderByDescending(t => t.CreatedUtc)
                .FirstOrDefaultAsync(ct);
        }

        public  Task UpdateAsync(PasswordResetToken token, CancellationToken ct)
        {
            _db.Update(token);
            return Task.CompletedTask;
        }
    }
}

