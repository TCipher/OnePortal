using OnePortal.Domain.Entities;

namespace OnePortal.Application.Abstractions
{
    public interface IPortalRoleRepository : IRepository<PortalRole>
    {
        new Task<PortalRole?> GetByIdAsync(int id, CancellationToken ct);
        Task<PortalRole?> GetByCodeAsync(int portalId, string code, CancellationToken ct);
        Task<IReadOnlyList<PortalRole>> GetByPortalAsync(int portalId, CancellationToken ct);
    }
}
