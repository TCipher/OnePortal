using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class PortalRoleConfiguration : IEntityTypeConfiguration<PortalRole>
    {
        public void Configure(EntityTypeBuilder<PortalRole> b)
        {
            b.ToTable("PortalRoles");
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.HasIndex(x => new { x.PortalId, x.Code }).IsUnique();

            b.HasOne(x => x.Portal)
             .WithMany(p => p.Roles)
             .HasForeignKey(x => x.PortalId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasData(SeedConstants.PortalRoles);
        }
    }
}
