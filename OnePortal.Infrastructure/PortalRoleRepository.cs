using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Persistence
{
    public class EfPortalRoleRepository : EfRepository<PortalRole>, IPortalRoleRepository
    {
        public EfPortalRoleRepository(OnePortalDbContext db) : base(db) { }

        public new async Task<PortalRole?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.PortalRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<PortalRole?> GetByCodeAsync(int portalId, string code, CancellationToken ct)
        {
            return await _db.PortalRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PortalId == portalId && r.Code == code, ct);
        }

        public async Task<IReadOnlyList<PortalRole>> GetByPortalAsync(int portalId, CancellationToken ct)
        {
            return await _db.PortalRoles
                .AsNoTracking()
                .Where(r => r.PortalId == portalId)
                .OrderBy(r => r.Name)
                .ToListAsync(ct);
        }
    }
}
