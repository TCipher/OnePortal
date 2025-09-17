using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Persistence
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly OnePortalDbContext _db;
        public EfRepository(OnePortalDbContext db) => _db = db;

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _db.Set<T>().FindAsync(new object?[] { id }, ct);

        public Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            _db.Set<T>().Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _db.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _db.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => _db.Set<T>().AnyAsync(predicate, ct);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
            => await _db.Set<T>().ToListAsync(ct);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _db.Set<T>().Where(predicate).ToListAsync(ct);
    }
}
