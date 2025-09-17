using OnePortal.Application.Abstractions;
using OnePortal.Application.Lookups.Dtos;
using OnePortal.Domain.Entities.Lookups;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Infrastructure.Persistence
{
    public class LookupRepository : ILookupRepository
{
    private readonly OnePortalDbContext _db;
    public LookupRepository(OnePortalDbContext db) => _db = db;

    public async Task<LookupSetsDto> GetAllAsync(CancellationToken ct)
     {
       var dto = new LookupSetsDto
        {
            Departments = await _db.Departments.AsNoTracking().Where(x => x.IsActive)
                 .OrderBy(x => x.Name)
                 .Select(x => new LookupItemDto { Id = x.Id, Code = x.Code, Name = x.Name
    })
               .ToListAsync(ct),
            Designations = await _db.Designations.AsNoTracking().Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItemDto { Id = x.Id, Code = x.Code, Name = x.Name
})
                .ToListAsync(ct),
SubDepartments = await _db.SubDepartments.AsNoTracking().Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItemDto { Id = x.Id, Code = x.Code, Name = x.Name })
                .ToListAsync(ct),

Nationalities = await _db.Nationalities.AsNoTracking().Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItemDto { Id = 0, Code = x.Code, Name = x.Name })
                .ToListAsync(ct)
        };
        return dto;
    }
}
}
