using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Persistence
{
    public class PortalRepository : EfRepository<Portal>, IPortalRepository
    {
        public PortalRepository(OnePortalDbContext db) : base(db) { }

        public async Task<Portal?> GetByCodeAsync(string code, CancellationToken ct)
        {
            return await _db.Portals
                .FirstOrDefaultAsync(p => p.Code == code, ct);
        }
    }
}
