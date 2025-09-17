using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Users.Dtos
{
    public class PortalDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }

    public class PortalAccessDto
    {
        public int PortalId { get; set; }
        public string PortalCode { get; set; } = default!;
        public string PortalName { get; set; } = default!;
        public int PortalRoleId { get; set; }
        public string PortalRoleCode { get; set; } = default!;
        public string PortalRoleName { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
