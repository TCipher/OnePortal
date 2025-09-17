using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Persistence;

public class UserPortalAccessRepository
    : EfRepository<UserPortalAccess>, IUserPortalAccessRepository
{
    public UserPortalAccessRepository(OnePortalDbContext db) : base(db) { }

    public async Task<UserPortalAccess?> GetByUserAndPortalAsync(int userId, int portalId, CancellationToken ct)
    {
        return await _db.UserPortalAccesses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.PortalId == portalId, ct);
    }

    public async Task<UserPortalAccess?> GetByUserAndPortalIncludingInactiveAsync(int userId, int portalId, CancellationToken ct)
    {
        return await _db.UserPortalAccesses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.PortalId == portalId, ct);
    }

    public async Task<List<UserPortalAccess>> GetUsersByPortalAsync(int portalId, CancellationToken ct)
    {
        return await _db.UserPortalAccesses
            .Where(a => a.PortalId == portalId)
            .Include(a => a.PortalRole)
            .Include(a => a.User)
            .OrderBy(a => a.User.LastName)
            .ThenBy(a => a.User.FirstName)
            .ToListAsync(ct);
    }

    public async Task<UserPortalAccess?> GetWithJoinsAsync(int id, CancellationToken ct)
    {
        return await _db.UserPortalAccesses
            .Include(a => a.Portal)
            .Include(a => a.PortalRole)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<UserPortalAccess?> GetWithJoinsByUserAndPortalAsync(int userId, int portalId, CancellationToken ct)
    {
        return await _db.UserPortalAccesses
            .Include(a => a.Portal)
            .Include(a => a.PortalRole)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.PortalId == portalId, ct);
    }

    public async Task<List<UserPortalAccess>> GetByUserWithJoinsAsync(int userId, CancellationToken ct)
    {
        return await _db.UserPortalAccesses
            .Where(a => a.UserId == userId)
            .Include(a => a.Portal)
            .Include(a => a.PortalRole)
            .OrderBy(a => a.Portal.Name)
            .ToListAsync(ct);
    }
}
