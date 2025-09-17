using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Application.AuditLogs.Dtos;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Persistence;

public class AuditLogRepository : EfRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(OnePortalDbContext db) : base(db) { }

    public async Task<List<AuditLogDto>> GetForUserAsync(int userId, int take, CancellationToken ct)
    {
        return await _db.AuditLogs
            .Where(a => a.EntityType == "UserDetails" && (a.EntityId == userId || a.ActorUserId == userId))
            .OrderByDescending(a => a.TimestampUtc)
            .Take(take)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                TimestampUtc = a.TimestampUtc,
                ActorUserId = a.ActorUserId,
                ActorEmail = a.ActorEmail,
                ActorRoleCode = a.ActorRoleCode,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                PortalId = a.PortalId,
                PortalCode = a.PortalCode,
                Success = a.Success,
                Message = a.Message
            })
            .ToListAsync(ct);
    }

    public async Task<List<AuditLogDto>> GetForPortalAsync(int portalId, int take, CancellationToken ct)
    {
        return await _db.AuditLogs
            .Where(a => a.PortalId == portalId)
            .OrderByDescending(a => a.TimestampUtc)
            .Take(take)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                TimestampUtc = a.TimestampUtc,
                ActorUserId = a.ActorUserId,
                ActorEmail = a.ActorEmail,
                ActorRoleCode = a.ActorRoleCode,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                PortalId = a.PortalId,
                PortalCode = a.PortalCode,
                Success = a.Success,
                Message = a.Message
            })
            .ToListAsync(ct);
    }
}
