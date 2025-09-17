using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePortal.Domain.Entities.Lookups;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations

{

    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> b)
        {
            b.ToTable("Department");
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();

            //b.HasData(SeedConstants.Portals);
        }
    }
}