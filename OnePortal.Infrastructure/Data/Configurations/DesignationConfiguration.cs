using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePortal.Domain.Entities.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
    {
        public void Configure(EntityTypeBuilder<Designation> b)
        {
            b.ToTable("Designation");
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Code).HasMaxLength(32);
            b.Property(x => x.Name).HasMaxLength(128);
        }
    }



}