using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Users.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? EmpCode { get; set; }
        public int? RoleId { get; set; }
        public string? RoleCode { get; set; }
        public string? RoleName { get; set; }
        public string EmailAddress { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? MiddleName { get; set; }
        public List<PortalAccessDto> PortalAccesses { get; set; } = new();
    }

    public class UserListItemDto
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
