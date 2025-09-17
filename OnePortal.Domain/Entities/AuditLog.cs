using OnePortal.Domain.Abstractions;

namespace OnePortal.Domain.Entities;

public class AuditLog : BaseEntity
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public int? ActorUserId { get; set; }              // who performed the action (nullable for system)
    public string? ActorEmail { get; set; }
    public string? ActorRoleCode { get; set; }         // SUPERADMIN / ADMIN / USER

    public string Action { get; set; } = default!;     // CREATE_USER, UPDATE_USER, ADD_PORTAL_ACCESS, etc.
    public string EntityType { get; set; } = default!; // "UserDetails", "UserPortalAccess", ...
    public int? EntityId { get; set; }              // optional (when we know the id)

    public int? PortalId { get; set; }                 // for portal-scoped operations
    public string? PortalCode { get; set; }

    public bool Success { get; set; } = true;
    public string? Message { get; set; }               // error text or info

    public string? DetailsJson { get; set; }           // serialized request/response subset
    public string? CorrelationId { get; set; }         // http trace id
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
