using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class PortalRole : BaseEntity
    {
        public int PortalId { get; set; }
        public Portal Portal { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public ICollection<UserPortalAccess> Accesses { get; set; } = new List<UserPortalAccess>();
    }
}
