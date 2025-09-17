using OnePortal.Domain.Entities;

namespace OnePortal.Application.Abstractions;

public interface IAuditLogger
{
    Task LogAsync(AuditLog entry, CancellationToken ct = default);
}
