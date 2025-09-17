using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Scheduling;

public class PasswordRotationJob
{
    private readonly OnePortalDbContext _db;
    private readonly IEmailSender _email;
    public PasswordRotationJob(OnePortalDbContext db, IEmailSender email) { _db = db; _email = email; }

    public async Task Run(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var users = await _db.Users.AsNoTracking().Select(u => new { u.Id, u.EmailAddress, u.PasswordLastChangedUtc, u.MustChangePassword }).ToListAsync(ct);

        foreach (var u in users)
        {
            var entity = await _db.Users.FirstAsync(x => x.Id == u.Id, ct);
            if (entity.PasswordLastChangedUtc is null) continue;

            var days = (now - entity.PasswordLastChangedUtc.Value).TotalDays;
            if (days >= 180 && !entity.MustChangePassword) entity.MustChangePassword = true;

            if (days >= 166 && days < 180)
            {
                await _email.SendAsync(entity.EmailAddress, "Password rotation reminder",
                    "<p>Your OnePortal password will expire soon. Please change it.</p>", ct);
            }
            await _db.SaveChangesAsync(ct);
        }
    }
}
