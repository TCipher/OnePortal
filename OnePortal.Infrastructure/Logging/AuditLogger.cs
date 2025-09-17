
using OnePortal.Infrastructure.Persistence;
using OnePortal.Application.Abstractions;

namespace OnePortal.Infrastructure.Logging;

public class AuditLogger : IAuditLogger
{
    private readonly IRepository<AuditLog> _repo;
    public AuditLogger(IRepository<AuditLog> repo) => _repo = repo;

    public Task LogAsync(AuditLog entry, CancellationToken ct = default) => _repo.AddAsync(entry, ct);
}
