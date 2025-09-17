using OnePortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Users.Dtos
{
    public class CreateUserRequest
    {
        public string? EmpCode { get; set; }
       public int? RoleId { get; set; } // Global role FK

        // Contact
        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        // Org structure / reporting
        public int ReportingManagerId { get; set; }
        public int DesignationId { get; set; }
        public int DepartmentId { get; set; }
        public int SubDepartmentId { get; set; }
        public string? OrganizationId { get; set; }

        // Dates
        public DateOnly? BirthDate { get; set; }
        public DateOnly? EngageDate { get; set; }
        public DateOnly? ReleaseDate { get; set; }

        // Enum-backed simple fields (map to short IDs in entity)
        [Required]
        public Gender Gender { get; set; } 
        public Level Level { get; set; }
        public WorkLocation WorkLocation { get; set; }

        // Lookup-backed dropdowns (DB IDs)
        public string? NationalityId { get; set; }
        public JobType JobType { get; set; }
        public SkillType SkillType { get; set; }

        // Address/extra
        public string? OriginState { get; set; }
        public string? LGA { get; set; }
        public string? Community { get; set; }

        // Leave
        public int? ALAllowed { get; set; }
        public int? ALTaken { get; set; }
        public int? CLAllowed { get; set; }
        public int? CLTaken { get; set; }

        // Status
        public bool IsReleased { get; set; }
        public bool IsUserUpdated { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsRSA { get; set; }
        public bool IsEmail { get; set; }
        public bool IsOneportalUser { get; set; } = true;

        // Names
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }

        // Signature (Base64 string from client; we’ll decode to byte[])
        public string? UserSignatureBase64 { get; set; }

        // MFA
        [Required] public MfaMethod SelectedMfa { get; set; } = MfaMethod.EmailOtp;

        // Initial portal accesses
        public List<PortalAccessDto> PortalAccesses { get; set; } = new();
    }
}
