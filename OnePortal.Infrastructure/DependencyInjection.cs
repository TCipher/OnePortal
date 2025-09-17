using Hangfire;
using Fido2NetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Configuration;
using OnePortal.Infrastructure.Data;
using OnePortal.Infrastructure.Logging;
using OnePortal.Infrastructure.Persistence;
using OnePortal.Infrastructure.Scheduling;
using OnePortal.Infrastructure.Security;

namespace OnePortal.Infrastructure;

public static class DependencyInjection
{
   
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<OnePortalDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("Default")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPortalRepository, PortalRepository>();
        services.AddScoped<IPortalRoleRepository, EfPortalRoleRepository>();
        services.AddScoped<IUserPortalAccessRepository, UserPortalAccessRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IWebAuthnCredentialRepository, WebAuthnCredentialRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // HttpContext & CurrentUser
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, JwtCurrentUser>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        // Security / Auth Services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
       services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();

        // SMTP / Email
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // Caching (challenge store for WebAuthn; swap to Redis in prod)
        services.AddDistributedMemoryCache();
        services.AddScoped<IChallengeStore, ChallengeStore>();

        // FIDO2 + WebAuthn provider
        services.Configure<Fido2Options>(configuration.GetSection("Fido2"));
        services.AddScoped<IFido2>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<Fido2Options>>().Value;
            return new Fido2(new Fido2Configuration
            {
                ServerDomain = opt.RpId,
                ServerName = opt.RpName,
                Origins = new HashSet<string>(opt.Origins ?? Array.Empty<string>())
            });
        });
services.AddScoped<IWebAuthnProvider, WebAuthnProvider>();
        services.AddScoped<ILookupRepository, LookupRepository>();

        return services;
    }

  
  public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
    services.AddHangfire(cfg => cfg
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("Default")));
    
    services.AddHangfireServer();
    services.AddScoped<PasswordRotationJob>();
    
            return services;
        }
}