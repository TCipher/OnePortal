using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities.Lookups
{
    public class SubDepartment : BaseEntity, ISoftDeletable
{
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
}
