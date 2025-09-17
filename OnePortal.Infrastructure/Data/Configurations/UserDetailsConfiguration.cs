using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OnePortal.Domain.Entities;

namespace OnePortal.Infrastructure.Data.Configurations
{
    public class UserDetailsConfiguration : IEntityTypeConfiguration<UserDetails>
    {
        public void Configure(EntityTypeBuilder<UserDetails> b)
        {
            b.ToTable("UsersDetails");

            // Keys & indexes
            b.HasIndex(x => x.EmailAddress).IsUnique();

            // Basic fields
            b.Property(x => x.EmailAddress).HasMaxLength(200).IsRequired();
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.EmpCode).HasMaxLength(50);

            // DateOnly? -> date
            var dateOnlyNullable = new ValueConverter<DateOnly?, DateTime?>(
                d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                dt => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : (DateOnly?)null);

            b.Property(x => x.BirthDate).HasConversion(dateOnlyNullable).HasColumnType("date");
            b.Property(x => x.EngageDate).HasConversion(dateOnlyNullable).HasColumnType("date");
            b.Property(x => x.ReleaseDate).HasConversion(dateOnlyNullable).HasColumnType("date");

            // Global role FK
            b.HasOne(x => x.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.SetNull);

            // Org lookups
            b.HasOne(u => u.Department)
             .WithMany()
             .HasForeignKey(u => u.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(u => u.SubDepartment)
             .WithMany()
             .HasForeignKey(u => u.SubDepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(u => u.Designation)
             .WithMany()
             .HasForeignKey(u => u.DesignationId)
             .OnDelete(DeleteBehavior.SetNull);

            // Nationality remains a lookup (keyed by string Code)
            b.HasOne(u => u.Nationality)
             .WithMany()
             .HasForeignKey(u => u.NationalityId)
             .OnDelete(DeleteBehavior.SetNull);

            // Enum-backed properties -> smallint
            b.Property(x => x.Gender)
             .HasConversion<short>()
             .HasColumnType("smallint");

            b.Property(x => x.WorkLocation)
             .HasConversion<short>()
             .HasColumnType("smallint");

            b.Property(x => x.Level)
             .HasConversion<short>()
             .HasColumnType("smallint");

            b.Property(x => x.PreferredMfaMethod)
             .HasConversion<short>()
             .HasColumnType("smallint");

            // Nullable enum mappings
            b.Property(x => x.JobType)
             .HasConversion<short?>()
             .HasColumnType("smallint");

            b.Property(x => x.SkillType)
             .HasConversion<short?>()
             .HasColumnType("smallint");

            // Collections
            b.HasMany(x => x.PortalAccesses)
             .WithOne(pa => pa.User)
             .HasForeignKey(pa => pa.UserId);
        }
    }
}
