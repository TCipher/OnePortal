using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class PortalConfiguration : IEntityTypeConfiguration<Portal>
    {
        public void Configure(EntityTypeBuilder<Portal> b)
        {
            b.ToTable("Portals");
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();

            b.HasData(SeedConstants.Portals);
        }
    }

}