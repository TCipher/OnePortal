using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.PortalAccess.Dtos
{
    public class UserPortalAccessItemDto
    {
        public int PortalId { get; set; }
        public string PortalCode { get; set; } = default!;
        public string PortalName { get; set; } = default!;
        public int PortalRoleId { get; set; }
        public string PortalRoleCode { get; set; } = default!;
        public string PortalRoleName { get; set; } = default!;
        public bool IsActive { get; set; }
    }

    public class PortalUserItemDto
    {
        public int UserId { get; set; }
        public string EmailAddress { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public int PortalRoleId { get; set; }
        public string PortalRoleCode { get; set; } = default!;
        public string PortalRoleName { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
