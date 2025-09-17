using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Lookups.Dtos
{
   public class LookupSetsDto
    { 
    public List<LookupItemDto> Departments { get; set; } = new();
    public List<LookupItemDto> Designations { get; set; } = new();
    public List<LookupItemDto> SubDepartments { get; set; } = new();
    public List<LookupItemDto> Nationalities { get; set; } = new(); 
}
}
