
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;
using OnePortal.Infrastructure.Data.Seed;
using OnePortal.Infrastructure.Scheduling;

namespace OnePortal.Infrastructure;

public static class StartupTasksExtensions
{
  
    public static async Task UseInfrastructureStartupAsync(this IApplicationBuilder app, CancellationToken ct = default)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            // Schedule recurring jobs via DI
            var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            recurring.AddOrUpdate<PasswordRotationJob>(
                "password-rotation",
                job => job.Run(CancellationToken.None),
                "0 2 * * *"); // daily 02:00 UTC
        }

        using (var scope = app.ApplicationServices.CreateScope())
            {
                // Seed database
    var db = scope.ServiceProvider.GetRequiredService<OnePortalDbContext>();
    var pwd = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    await DbSeeder.SeedAsync(db, pwd);
            }
    }
}

