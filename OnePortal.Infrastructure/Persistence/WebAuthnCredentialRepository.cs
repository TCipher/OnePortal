using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Persistence;

public class WebAuthnCredentialRepository
    : EfRepository<WebAuthnCredential>, IWebAuthnCredentialRepository
{
    public WebAuthnCredentialRepository(OnePortalDbContext db) : base(db) { }

    public Task<WebAuthnCredential?> GetActiveByCredentialIdAsync(string credentialId, CancellationToken ct = default)
        => _db.WebAuthnCredentials
              .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.IsActive, ct);

    public Task<WebAuthnCredential?> GetActiveByCredentialIdForUserAsync(string credentialId, int userId, CancellationToken ct = default)
        => _db.WebAuthnCredentials
              .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.UserId == userId && c.IsActive, ct);

    public Task<List<WebAuthnCredential>> ListActiveByUserAsync(int userId, CancellationToken ct = default)
        => _db.WebAuthnCredentials
              .Where(c => c.UserId == userId && c.IsActive)
              .ToListAsync(ct);

    public Task<List<WebAuthnCredential>> GetActiveByUserAsync(int userId, CancellationToken ct = default)
        => _db.WebAuthnCredentials.Where(c => c.UserId == userId && c.IsActive).ToListAsync(ct);
}
