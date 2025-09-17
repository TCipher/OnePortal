using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IWebAuthnCredentialRepository : IRepository<WebAuthnCredential>
    {
        Task<WebAuthnCredential?> GetActiveByCredentialIdAsync(string credentialId, CancellationToken ct = default);
        Task<WebAuthnCredential?> GetActiveByCredentialIdForUserAsync(string credentialId, int userId, CancellationToken ct = default);
        Task<List<WebAuthnCredential>> ListActiveByUserAsync(int userId, CancellationToken ct = default);
        Task<List<WebAuthnCredential>> GetActiveByUserAsync(int userId, CancellationToken ct = default);
    }
}
