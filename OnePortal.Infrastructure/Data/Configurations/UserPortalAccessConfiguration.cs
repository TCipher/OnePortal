using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePortal.Domain.Entities;

namespace OnePortal.Infrastructure.Data.Configurations;

public class UserPortalAccessConfiguration : IEntityTypeConfiguration<UserPortalAccess>
{
    public void Configure(EntityTypeBuilder<UserPortalAccess> b)
    {
        b.ToTable("UserPortalAccess");

        b.HasKey(x => x.Id);

        // Uniqueness: a user can appear only once per portal
        b.HasIndex(x => new { x.UserId, x.PortalId }).IsUnique();

        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.AssignedOn).HasDefaultValueSql("GETUTCDATE()");

        // IMPORTANT: prevent multiple cascade paths
        b.HasOne(x => x.Portal)
     .WithMany(p => p.Accesses)
     .HasForeignKey(x => x.PortalId)
     .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.PortalRole)
            .WithMany(r => r.Accesses)
            .HasForeignKey(x => x.PortalRoleId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.User)
            .WithMany(u => u.PortalAccesses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
