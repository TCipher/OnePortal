using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> b)
        {
            b.ToTable("PasswordResetTokens");
            b.HasIndex(x => new { x.UserId, x.TokenHash });
            b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        }
    }
}
