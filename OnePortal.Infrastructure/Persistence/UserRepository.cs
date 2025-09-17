using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Common;
using OnePortal.Application.Users.Dtos;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;

namespace OnePortal.Infrastructure.Persistence;

public class UserRepository : EfRepository<UserDetails>, IUserRepository
{
    public UserRepository(OnePortalDbContext db) : base(db) { }

    public async Task<UserDetails?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.EmailAddress == email, ct);
    }

    public async Task<UserDetails?> GetWithAccessesAsync(int id, CancellationToken ct)
    {
        return await _db.Users
            .Include(u => u.Role)
            .Include(u => u.PortalAccesses).ThenInclude(a => a.Portal)
            .Include(u => u.PortalAccesses).ThenInclude(a => a.PortalRole)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<PagedResult<UserListItemDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Users.Include(u => u.Role).OrderBy(u => u.Id);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                EmailAddress = u.EmailAddress,
                FullName = (u.FirstName + " " +
                           (u.MiddleName != null ? u.MiddleName + " " : "") +
                            u.LastName).Trim(),
                RoleName = u.Role != null ? u.Role.Name : null,
                IsActive = u.IsActive
            })
            .ToListAsync(ct);

        return new PagedResult<UserListItemDto>(items, page, pageSize, total);
    }

}
