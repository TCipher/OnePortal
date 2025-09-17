using OnePortal.Application.Common;
using OnePortal.Application.Users.Dtos;
using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IUserRepository : IRepository<UserDetails>
    {
        Task<UserDetails?> GetByEmailAsync(string email, CancellationToken ct);
        Task<UserDetails?> GetWithAccessesAsync(int id, CancellationToken ct);
        Task<PagedResult<UserListItemDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
    }
}
