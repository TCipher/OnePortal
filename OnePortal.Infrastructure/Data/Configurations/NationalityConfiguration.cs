using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnePortal.Domain.Entities.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class NationalityConfiguration : IEntityTypeConfiguration<Nationality>
    {
        public void Configure(EntityTypeBuilder<Nationality> b) 
        {
            b.ToTable("Nationality");
            b.HasKey(x => x.Code);
            b.Property(x => x.Code).HasMaxLength(3);
            b.Property(x => x.Name).HasMaxLength(128);
        }
    }
}
