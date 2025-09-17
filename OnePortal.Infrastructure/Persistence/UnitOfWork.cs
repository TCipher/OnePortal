using Microsoft.EntityFrameworkCore.Storage;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OnePortalDbContext _db;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        private IUserRepository? _users;
        private IPortalRepository? _portals;
        private IPortalRoleRepository? _portalRoles;
        private IUserPortalAccessRepository? _userPortalAccesses;
        private IAuditLogRepository? _auditLogs;

        public UnitOfWork(OnePortalDbContext db)
        {
            _db = db;
        }

        public IUserRepository Users => _users ??= new UserRepository(_db);
        public IPortalRepository Portals => _portals ??= new PortalRepository(_db);
        public IPortalRoleRepository PortalRoles => _portalRoles ??= new EfPortalRoleRepository(_db);
        public IUserPortalAccessRepository UserPortalAccesses => _userPortalAccesses ??= new UserPortalAccessRepository(_db);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_db);

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            return new EfRepository<T>(_db);
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _db.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");
            
            _transaction = await _db.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            try
            {
                await _db.SaveChangesAsync(ct);
                await _transaction.CommitAsync(ct);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to rollback");

            try
            {
                await _transaction.RollbackAsync(ct);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _db.Dispose();
                _disposed = true;
            }
        }
    }
}
