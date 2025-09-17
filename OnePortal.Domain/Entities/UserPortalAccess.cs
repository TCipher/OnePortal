using OnePortal.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Domain.Entities
{
    public class UserPortalAccess : BaseEntity, ISoftDeletable
    {
        public int UserId { get; set; }
        public UserDetails User { get; set; } = default!;
        public int PortalId { get; set; }
        public Portal Portal { get; set; } = default!;
        public int PortalRoleId { get; set; }
        public PortalRole PortalRole { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public DateTime AssignedOn { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedOn { get; set; }
    }
}
