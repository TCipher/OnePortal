using OnePortal.Application.Lookups.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface ILookupRepository
{
    Task<LookupSetsDto> GetAllAsync(CancellationToken ct);
}
}
