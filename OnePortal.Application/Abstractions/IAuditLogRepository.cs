using OnePortal.Application.AuditLogs.Dtos;
using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<List<AuditLogDto>> GetForUserAsync(int userId, int take, CancellationToken ct);
        Task<List<AuditLogDto>> GetForPortalAsync(int portalId, int take, CancellationToken ct);
    }
}
