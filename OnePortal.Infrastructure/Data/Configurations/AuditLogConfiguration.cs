using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnePortal.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.Property(x => x.Action).HasMaxLength(120).IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(120).IsRequired();
        b.Property(x => x.ActorEmail).HasMaxLength(200);
        b.Property(x => x.ActorRoleCode).HasMaxLength(50);
        b.Property(x => x.PortalCode).HasMaxLength(50);
        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.ActorUserId);
        b.HasIndex(x => x.PortalId);
        b.HasIndex(x => x.TimestampUtc);
    }
}
