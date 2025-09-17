namespace OnePortal.Application.AuditLogs.Dtos;

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorEmail { get; set; }
    public string? ActorRoleCode { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public int? EntityId { get; set; }
    public int? PortalId { get; set; }
    public string? PortalCode { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
