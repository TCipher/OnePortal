using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data.Seed;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OnePortal.Infrastructure.Scheduling
{
    public class StartupDataSeederHostedService : IHostedService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<StartupDataSeederHostedService> _log;

        public StartupDataSeederHostedService(IServiceProvider sp, ILogger<StartupDataSeederHostedService> log)
        {
            _sp = sp; _log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OnePortalDbContext>();
            var pwd = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            // Ensure DB is up-to-date before seeding (optional if you migrate elsewhere)
            await db.Database.EnsureCreatedAsync(cancellationToken);

            _log.LogInformation("Running runtime seeder...");
            await DbSeeder.SeedAsync(db, pwd, cancellationToken);
            _log.LogInformation("Runtime seeder finished.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
