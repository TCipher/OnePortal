using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.WebAuthn.Dtos
{
    public record WebAuthnCredentialDto(
     string CredentialId,
     string? Aaguid,
     DateTime CreatedUtc,
     uint SignCount,
     string[] Transports
 );
}
