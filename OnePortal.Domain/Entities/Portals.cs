using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class Portal : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public ICollection<PortalRole> Roles { get; set; } = new List<PortalRole>();
        public ICollection<UserPortalAccess> Accesses { get; set; } = new List<UserPortalAccess>();
    }
}
