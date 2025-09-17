using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Domain.Entities.Lookups;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Data.Seed
{
    public class DbSeeder
    {
        public static async Task SeedAsync(OnePortalDbContext db, IPasswordService pwd, CancellationToken ct = default)
        {
            // Global roles
            if (!await db.GlobalRoles.AnyAsync(ct))
            {
                db.GlobalRoles.AddRange(
                    new GlobalRole { Code = GlobalRoleCodes.SuperAdmin, Name = "Super Admin" },
                    new GlobalRole { Code = GlobalRoleCodes.Admin, Name = "Admin" },
                    new GlobalRole { Code = GlobalRoleCodes.User, Name = "User" }
                );
            }

            // Lookups (departments, designations, etc.)
            await SeedLookupsAsync(db, ct);

            // Ensure 15 default portals & portal roles already exist from earlier sprint seeding.

            // Super Admin user
            if (!await db.Users.AnyAsync(u => u.EmailAddress == "tochukwu.ndefo@tolaram.com", ct))
            {
                db.Users.Add(new UserDetails
                {
                    EmailAddress = "tochukwu.ndefo@tolaram.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    RoleId = await db.GlobalRoles
                        .Where(r => r.Code == GlobalRoleCodes.SuperAdmin)
                        .Select(r => r.Id)
                        .FirstAsync(ct),
                    DepartmentId = await db.Departments
                        .Where(d => d.Code == "SIMS")
                        .Select(d => d.Id)
                        .FirstAsync(ct),
                    PasswordHash = pwd.Hash("Sup3r@dm1n!"),
                    PasswordLastChangedUtc = DateTime.UtcNow,
                    PreferredMfaMethod = MfaMethod.EmailOtp,
                    IsActive = true
                });
            }

            // Regular user
            if (!await db.Users.AnyAsync(u => u.EmailAddress == "tochind29@gmail.com", ct))
            {
                db.Users.Add(new UserDetails
                {
                    EmailAddress = "tochind29@gmail.com",
                    FirstName = "Regular",
                    LastName = "User",
                    RoleId = await db.GlobalRoles
                        .Where(r => r.Code == GlobalRoleCodes.User)
                        .Select(r => r.Id)
                        .FirstAsync(ct),
                    DepartmentId = await db.Departments
                        .Where(d => d.Code == "HR")
                        .Select(d => d.Id)
                        .FirstAsync(ct),
                    PasswordHash = pwd.Hash("User@12345"),
                    PasswordLastChangedUtc = DateTime.UtcNow,
                    PreferredMfaMethod = MfaMethod.EmailOtp,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task SeedLookupsAsync(OnePortalDbContext db, CancellationToken ct = default)
        {
            if (!await db.Departments.AnyAsync(ct))
            {
                db.Departments.AddRange(
                    new Department { Code = "SIMS", Name = "Sustainability" },
                    new Department { Code = "HR", Name = "Human Resources" },
                    new Department { Code = "FIN", Name = "Finance" }
                );
                await db.SaveChangesAsync(ct);
            }

            if (!await db.Designations.AnyAsync(ct))
            {
                db.Designations.AddRange(
                    new Designation { Code = "DR", Name = "Director" },
                    new Designation { Code = "SSE", Name = "Senior Software Engineer" },
                    new Designation { Code = "MN", Name = "Manager" }
                );
            }

            if (!await db.SubDepartments.AnyAsync(ct))
            {
                var engId = db.Departments.Local
                    .First(d => d.Code == "SIMS").Id;

                db.SubDepartments.AddRange(
                    new SubDepartment { DepartmentId = engId, Code = "SNR", Name = "Senior Manager" },
                    new SubDepartment { DepartmentId = engId, Code = "MN", Name = "Manager" }
                );
                await db.SaveChangesAsync(ct);
            }



            if (!await db.Nationalities.AnyAsync(ct))
            {
                db.Nationalities.AddRange(
                    new Nationality { Code = "NG", Name = "Nigeria" },
                    new Nationality { Code = "US", Name = "United States" },
                    new Nationality { Code = "GB", Name = "United Kingdom" }
                );

            }
        }
    }
}
