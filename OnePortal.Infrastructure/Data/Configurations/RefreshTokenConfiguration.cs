using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokens");
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        }
    }
}
