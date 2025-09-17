using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Lookups.Dtos
{
    public class LookupItemDto
{
    public int Id { get; set; }              
    public string? Code { get; set; }
    public string Name { get; set; } = default!;
}
}
