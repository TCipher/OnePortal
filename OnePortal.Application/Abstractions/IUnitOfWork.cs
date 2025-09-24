using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IPortalRepository Portals { get; }
        IPortalRoleRepository PortalRoles { get; }
        IUserPortalAccessRepository UserPortalAccesses { get; }
        IAuditLogRepository AuditLogs { get; }
        IWebAuthnCredentialRepository WebAuthnCredentials { get; }
        IRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}
