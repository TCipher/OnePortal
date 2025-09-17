using OnePortal.Domain.Entities.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Data
{
    public class OnePortalDbContext : DbContext
    {
        public OnePortalDbContext(DbContextOptions<OnePortalDbContext> options) : base(options) { }

        public DbSet<UserDetails> Users => Set<UserDetails>();
        public DbSet<GlobalRole> GlobalRoles => Set<GlobalRole>();
        public DbSet<Portal> Portals => Set<Portal>();
        public DbSet<PortalRole> PortalRoles => Set<PortalRole>();
        public DbSet<UserPortalAccess> UserPortalAccesses => Set<UserPortalAccess>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<WebAuthnCredential> WebAuthnCredentials => Set<WebAuthnCredential>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Designation> Designations => Set<Designation>();
        public DbSet<SubDepartment> SubDepartments => Set<SubDepartment>();
        public DbSet<Nationality> Nationalities => Set<Nationality>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OnePortalDbContext).Assembly);

            // Apply global soft-delete filter for all ISoftDeletable entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ISoftDeletable.IsActive));
                    var condition = Expression.Equal(prop, Expression.Constant(true));
                    var lambda = Expression.Lambda(condition, parameter);
                    entityType.SetQueryFilter(lambda);
                }
            }

            // Apply matching filters for single navigations to UserDetails
            var userDetailsType = typeof(UserDetails);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType == userDetailsType) continue;

                foreach (var navigation in entityType.GetNavigations()
                                                    .Where(n => n.TargetEntityType.ClrType == userDetailsType && !n.IsCollection))
                {
                    var param = Expression.Parameter(entityType.ClrType, "e");
                    var userProp = Expression.Property(param, navigation.Name);

                    // e.UserDetails != null && e.UserDetails.IsActive
                    var notNull = Expression.NotEqual(userProp, Expression.Constant(null, userDetailsType));
                    var isActiveProp = Expression.Property(userProp, nameof(ISoftDeletable.IsActive));
                    var activeCheck = Expression.Equal(isActiveProp, Expression.Constant(true));

                    var combined = Expression.AndAlso(notNull, activeCheck);
                    var lambda = Expression.Lambda(combined, param);

                    entityType.SetQueryFilter(lambda);
                }
            }

            // Prevent cascade delete -> use NoAction
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                if (!fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                    fk.DeleteBehavior = DeleteBehavior.NoAction;
            }

            base.OnModelCreating(modelBuilder);
        }


    }
}
