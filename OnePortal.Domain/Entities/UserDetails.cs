using OnePortal.Domain.Abstractions;
using OnePortal.Domain.Entities;
using OnePortal.Domain.Entities.Lookups;

public class UserDetails : BaseEntity, ISoftDeletable
{
    public string? EmpCode { get; set; }

    // Global role
    public int? RoleId { get; set; }
    public GlobalRole? Role { get; set; }

    // Contact
    public string EmailAddress { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Org / reporting
    public int? ReportingManagerId { get; set; }

    public int? DesignationId { get; set; }
    public Designation? Designation { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public int? SubDepartmentId { get; set; }
    public SubDepartment? SubDepartment { get; set; }
    public DateOnly? EngageDate { get; set; }

    public Level Level { get; set; }
    public WorkLocation WorkLocation { get; set; }

    public string? NationalityId { get; set; }
    public Nationality? Nationality { get; set; }
    public JobType? JobType { get; set; }
    public SkillType? SkillType { get; set; }

    public Gender Gender { get; set; }

    // Address / extras
    public string? OriginState { get; set; }
    public string? LGA { get; set; }
    public string? Community { get; set; }

    // Leave
    public int? ALAllowed { get; set; }
    public int? ALTaken { get; set; }
    public int? CLAllowed { get; set; }
    public int? CLTaken { get; set; }

    // Status & audit (domain-level)
    public bool IsReleased { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public bool IsUserUpdated { get; set; }
    public string? Createdby { get; set; }
    public DateTime Createddate { get; set; } = DateTime.UtcNow;
    public string? Modifiedby { get; set; }
    public DateTime? Modifieddate { get; set; }
    public bool IsActive { get; set; } = true;

    // Names
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    // Misc
    public bool IsRSA { get; set; }
    public bool IsEmail { get; set; }
    public string? OrganizationId { get; set; }
    public byte[]? UserSignature { get; set; }
    public bool IsOneportalUser { get; set; } = true;

    public ICollection<UserPortalAccess> PortalAccesses { get; set; } = new List<UserPortalAccess>();

    // Auth
    public string? PasswordHash { get; set; }
    public DateTime? PasswordLastChangedUtc { get; set; }
    public bool MustChangePassword { get; set; }

    // CHANGED: enum type + proper default
    public MfaMethod PreferredMfaMethod { get; set; } = MfaMethod.EmailOtp;

    public DateTime? MfaEnrolledUtc { get; set; }

    // Email OTP
    public string? EmailOtpHash { get; set; }
    public DateTime? EmailOtpExpiresUtc { get; set; }
    public DateTime? EmailOtpLastSentUtc { get; set; }
    public short EmailOtpFailedCount { get; set; }

    // Lockout
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public bool LockoutEnabled { get; set; } = true;
}