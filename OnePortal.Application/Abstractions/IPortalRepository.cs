using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IPortalRepository : IRepository<Portal>
    {
        Task<Portal?> GetByCodeAsync(string code, CancellationToken ct);
    }
}
