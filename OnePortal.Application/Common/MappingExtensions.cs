using OnePortal.Application.Users.Dtos;
using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Common
{
    public static class MappingExtensions
    {
        public static UserDto ToDto(this UserDetails u) =>
            new UserDto
            {
                Id = u.Id,
                EmpCode = u.EmpCode,
                EmailAddress = u.EmailAddress,
                PhoneNumber = u.PhoneNumber,
                FirstName = u.FirstName,
                LastName = u.LastName,
                MiddleName = u.MiddleName,
                RoleId = u.RoleId,
                RoleCode = u.Role?.Code,
                RoleName = u.Role?.Name,
                IsActive = u.IsActive,
                PortalAccesses = u.PortalAccesses.Select(pa => new PortalAccessDto
                {
                    PortalId = pa.PortalId,
                    PortalCode = pa.Portal.Code,
                    PortalName = pa.Portal.Name,
                    PortalRoleId = pa.PortalRoleId,
                    PortalRoleCode = pa.PortalRole.Code,
                    PortalRoleName = pa.PortalRole.Name,
                    IsActive = pa.IsActive
                }).ToList()
            };
    }
}
