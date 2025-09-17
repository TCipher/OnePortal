using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class WebAuthnCredential : BaseEntity
    {
        public int UserId { get; set; }
        public UserDetails User { get; set; } = default!;
        public string CredentialId { get; set; } = default!; // base64url
        public byte[] PublicKey { get; set; } = default!;
        public uint SignCount { get; set; }
        public Guid Aaguid { get; set; }
        public byte[] UserHandle { get; set; } = default!;
        public string? TransportsCsv { get; set; }
        public string? AttestationFmt { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
