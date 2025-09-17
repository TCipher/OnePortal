using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;
using OnePortal.Infrastructure.Persistence;

public class PermissionService : IPermissionService
{
    private readonly OnePortalDbContext _db;
    private readonly ICurrentUser _current;

    public PermissionService(OnePortalDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<bool> CanManagePortalAsync(int portalId, CancellationToken ct)
    {
        if (_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin))
            return true;

        return await _db.UserPortalAccesses
            .AnyAsync(a =>
                a.UserId == _current.UserId &&
                a.PortalId == portalId &&
                a.IsActive &&
                a.PortalRole.Code == "PORTAL_ADMIN", ct);
    }


    public Task<bool> CanManageUsersAsync(CancellationToken ct)
    {
        // Global SuperAdmin or Admin check
        if (_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin) ||
            _current.IsInGlobalRole(GlobalRoleCodes.Admin))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

}
