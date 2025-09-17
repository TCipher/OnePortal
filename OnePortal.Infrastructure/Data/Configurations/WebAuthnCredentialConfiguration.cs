using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class WebAuthnCredentialConfiguration : IEntityTypeConfiguration<WebAuthnCredential>
    {
        public void Configure(EntityTypeBuilder<WebAuthnCredential> b)
        {
            b.ToTable("WebAuthnCredentials");
            b.HasIndex(x => x.CredentialId).IsUnique();
            b.Property(x => x.CredentialId).HasMaxLength(500).IsRequired();
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        }
    }
}
