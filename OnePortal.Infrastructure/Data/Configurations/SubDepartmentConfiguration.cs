using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePortal.Domain.Entities.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class SubDepartmentConfiguration : IEntityTypeConfiguration<SubDepartment>
    {
        public void Configure(EntityTypeBuilder<SubDepartment> b)
        {
            b.ToTable("SubDepartment");
            b.Property(x => x.Code).HasMaxLength(32);
            b.Property(x => x.Name).HasMaxLength(128);
            b.HasOne(x => x.Department)
                           .WithMany()
                           .HasForeignKey(x => x.DepartmentId)
                            .OnDelete(DeleteBehavior.Restrict); ;
        }
    }
}
