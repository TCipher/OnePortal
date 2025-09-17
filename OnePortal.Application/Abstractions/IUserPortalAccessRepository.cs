using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

public interface IUserPortalAccessRepository : IRepository<UserPortalAccess>
{
    Task<UserPortalAccess?> GetByUserAndPortalAsync(int userId, int portalId, CancellationToken ct);
    Task<UserPortalAccess?> GetWithJoinsAsync(int id, CancellationToken ct);
    Task<UserPortalAccess?> GetWithJoinsByUserAndPortalAsync(int userId, int portalId, CancellationToken ct);
    Task<UserPortalAccess?> GetByUserAndPortalIncludingInactiveAsync(int userId, int portalId, CancellationToken ct);

    Task<List<UserPortalAccess>> GetUsersByPortalAsync(int portalId, CancellationToken ct);
    Task<List<UserPortalAccess>> GetByUserWithJoinsAsync(int userId, CancellationToken ct);

}
