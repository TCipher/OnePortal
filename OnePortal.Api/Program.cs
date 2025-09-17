using System.Text;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OnePortal.Application;
using OnePortal.Application.Abstractions;

using OnePortal.Domain.Entities;
using Serilog;
using OnePortal.API.Middleware;
using OnePortal.Infrastructure;
using System.Text.Json.Serialization;
using OnePortal.Infrastructure.Data.Seed;
using OnePortal.Infrastructure.Data;



var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBackgroundJobs(builder.Configuration);
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// -------------------------------
// Rate Limiting
// -------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("LoginLimiter", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("OtpLimiter", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
});

// -------------------------------
// Authentication (JWT)
// -------------------------------
//var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new()
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(key),
//            ClockSkew = TimeSpan.FromMinutes(1)
//        };
//    });

//// -------------------------------
//// Authorization policies
//// -------------------------------
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("SuperAdminOnly", policy =>
//        policy.RequireAssertion(ctx =>
//            ctx.User.HasClaim(c => c.Type == "global_role" && c.Value.Equals(GlobalRoleCodes.SuperAdmin, StringComparison.OrdinalIgnoreCase))
//            || ctx.User.IsInRole(GlobalRoleCodes.SuperAdmin)));

//    options.AddPolicy("AdminOrSuperAdmin", policy =>
//        policy.RequireAssertion(ctx =>
//            ctx.User.IsInRole(GlobalRoleCodes.SuperAdmin) ||
//            ctx.User.IsInRole(GlobalRoleCodes.Admin) ||
//            string.Equals(ctx.User.FindFirst("global_role")?.Value, GlobalRoleCodes.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
//            string.Equals(ctx.User.FindFirst("global_role")?.Value, GlobalRoleCodes.Admin, StringComparison.OrdinalIgnoreCase)));
//});
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // 👇 Add logging hooks
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ctx.Exception, "JWT Authentication failed. Token: {Token}", ctx.Request.Headers["Authorization"].ToString());
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Challenge triggered. Error: {Error}, Description: {Description}", ctx.Error, ctx.ErrorDescription);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var claims = string.Join(", ", ctx.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? []);
                logger.LogInformation("JWT validated successfully for user {Sub}. Claims: {Claims}", ctx.Principal?.FindFirst("sub")?.Value, claims);
                return Task.CompletedTask;
            }
        };
    });
 builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(c => c.Type == "global_role" && c.Value.Equals(GlobalRoleCodes.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            || ctx.User.IsInRole(GlobalRoleCodes.SuperAdmin)));

    options.AddPolicy("AdminOrSuperAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(GlobalRoleCodes.SuperAdmin) ||
            ctx.User.IsInRole(GlobalRoleCodes.Admin) ||
            string.Equals(ctx.User.FindFirst("global_role")?.Value, GlobalRoleCodes.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ctx.User.FindFirst("global_role")?.Value, GlobalRoleCodes.Admin, StringComparison.OrdinalIgnoreCase)));
});
// -------------------------------
// MVC + Swagger
// -------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); ;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// -------------------------------
// CORS for SPA (WebAuthn requires HTTPS/origin)
// -------------------------------
builder.Services.AddCors(o => o.AddPolicy("spa", p => p
    .WithOrigins("https://app.oneportal.local")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OnePortalDbContext>();
    var pwd = scope.ServiceProvider.GetRequiredService<IPasswordService>();

    // Apply any pending migrations
    await db.Database.MigrateAsync();

    // Seed data (global roles, lookups, users, etc.)
    await DbSeeder.SeedAsync(db, pwd);
}
// -------------------------------
// Pipeline
// -------------------------------
app.UseRateLimiter();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("spa");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.Run();


// DevCurrentUser (optional for local testing)
// Keep JwtCurrentUser as the DI-registered implementation in production.
public class DevCurrentUser : ICurrentUser
{
    public int? UserId => 1;
    public bool IsInGlobalRole(string roleCode) => true; // grants all
}
