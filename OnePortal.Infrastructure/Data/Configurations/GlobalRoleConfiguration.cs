using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class GlobalRoleConfiguration : IEntityTypeConfiguration<GlobalRole>
    {
        public void Configure(EntityTypeBuilder<GlobalRole> b)
        {
            b.ToTable("GlobalRoles");
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();

            b.HasData(
                new GlobalRole { Id = 1, Name = "Super Admin", Code = GlobalRoleCodes.SuperAdmin },
                new GlobalRole { Id = 2, Name = "Admin", Code = GlobalRoleCodes.Admin },
                new GlobalRole { Id = 3, Name = "User", Code = GlobalRoleCodes.User }
            );
        }
    }
}
