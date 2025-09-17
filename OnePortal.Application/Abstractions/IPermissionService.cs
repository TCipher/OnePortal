namespace OnePortal.Application.Abstractions;

public interface IPermissionService
{
    Task<bool> CanManagePortalAsync(int portalId, CancellationToken ct = default);
    Task<bool> CanManageUsersAsync(CancellationToken ct);
}
