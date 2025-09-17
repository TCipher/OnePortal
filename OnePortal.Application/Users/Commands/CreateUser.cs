using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Users.Dtos;
using OnePortal.Domain.Entities;
using OnePortal.Application.Common;

namespace OnePortal.Application.Users.Commands;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserDto>;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Request.EmailAddress).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);

        // Enums
        RuleFor(x => x.Request.Gender).IsInEnum();
        RuleFor(x => x.Request.WorkLocation).IsInEnum();
        RuleFor(x => x.Request.SelectedMfa).IsInEnum();
        RuleFor(x => x.Request.Level).IsInEnum();
        RuleFor(x => x.Request.SkillType).IsInEnum();
        RuleFor(x => x.Request.JobType).IsInEnum();

        // Optional foreign keys must be positive
        RuleFor(x => x.Request.RoleId).GreaterThan(0).When(x => x.Request.RoleId.HasValue);
        RuleFor(x => x.Request.DesignationId).GreaterThan(0);
        RuleFor(x => x.Request.DepartmentId).GreaterThan(0);
        RuleFor(x => x.Request.SubDepartmentId).GreaterThan(0);
        // Level/WorkLocation are enums mapped to short?, leave as-is if null
    }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly ICurrentUser _current;
    private readonly IPermissionService _perm;
    private readonly IUserRepository _users;
    private readonly IPortalRepository _portals;
    private readonly IPortalRoleRepository _portalRoles;
    private readonly IUserPortalAccessRepository _userPortalAccesses;
    private readonly IUnitOfWork _uow;

    public CreateUserHandler(
        ICurrentUser current,
        IPermissionService perm,
        IUserRepository users,
        IPortalRepository portals,
        IPortalRoleRepository portalRoles,
        IUserPortalAccessRepository userPortalAccesses,
        IUnitOfWork uow)
    {
        _current = current;
        _perm = perm;
        _users = users;
        _portals = portals;
        _portalRoles = portalRoles;
        _userPortalAccesses = userPortalAccesses;
        _uow = uow;
    }

    public async Task<UserDto> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var req = cmd.Request;

        // AuthZ
        if (!await _perm.CanManageUsersAsync(ct))
            throw new UnauthorizedAccessException("You do not have permission to create users.");

        if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin) &&
            !_current.IsInGlobalRole(GlobalRoleCodes.Admin))
            throw new UnauthorizedAccessException("Only SuperAdmin or Admin can create users.");

        // Unique email
        if (await _users.GetByEmailAsync(req.EmailAddress, ct) is not null)
            throw new InvalidOperationException("Email already exists.");

        UserDetails entity = null;

        try
        {
            entity = new UserDetails
            {
                EmpCode = req.EmpCode,
                RoleId = req.RoleId,

                EmailAddress = req.EmailAddress?.Trim(),
                PhoneNumber = req.PhoneNumber,
                IsActive = req.IsActive,

                FirstName = req.FirstName?.Trim(),
                LastName = req.LastName?.Trim(),
                MiddleName = req.MiddleName?.Trim(),

                ReportingManagerId = req.ReportingManagerId,
                DesignationId = req.DesignationId,
                DepartmentId = req.DepartmentId,
                SubDepartmentId = req.SubDepartmentId,
                OrganizationId = req.OrganizationId,

                BirthDate = req.BirthDate,
                EngageDate = req.EngageDate,
                ReleaseDate = req.ReleaseDate,

                Gender = req.Gender,
                Level = req.Level,
                WorkLocation = req.WorkLocation,

                NationalityId = req.NationalityId,
                JobType = req.JobType,
                SkillType = req.SkillType,

                OriginState = req.OriginState,
                LGA = req.LGA,
                Community = req.Community,

                ALAllowed = req.ALAllowed,
                ALTaken = req.ALTaken,
                CLAllowed = req.CLAllowed,
                CLTaken = req.CLTaken,

                IsReleased = req.IsReleased,
                IsUserUpdated = req.IsUserUpdated,
                IsRSA = req.IsRSA,
                IsEmail = req.IsEmail,
                IsOneportalUser = req.IsOneportalUser,

                UserSignature = !string.IsNullOrWhiteSpace(req.UserSignatureBase64)
                    ? Convert.FromBase64String(req.UserSignatureBase64)
                    : null,

                PreferredMfaMethod = req.SelectedMfa,
                MustChangePassword = true,
                PasswordLastChangedUtc = null,
                Createddate = DateTime.UtcNow
            };
        }
        catch (FormatException ex)
        {
            // Likely a bad Base64 string
            throw new InvalidOperationException($"Invalid Base64 for UserSignature: {req.UserSignatureBase64}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error mapping CreateUserRequest to UserDetails. " +
                                                $"Payload: {System.Text.Json.JsonSerializer.Serialize(req)}", ex);
        }


        // Persist user
        entity = await _users.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        // Default SIMS/EMPLOYEE access
        var sims = await _portals.GetByCodeAsync("SIMS", ct)
                   ?? throw new KeyNotFoundException("Default SIMS portal not found.");
        var employeeRole = await _portalRoles.GetByCodeAsync(sims.Id, "EMPLOYEE", ct)
                          ?? throw new KeyNotFoundException("EMPLOYEE role not found in SIMS portal.");

        await _userPortalAccesses.AddAsync(new UserPortalAccess
        {
            UserId = entity.Id,
            PortalId = sims.Id,
            PortalRoleId = employeeRole.Id,
            IsActive = true
        }, ct);
        await _uow.SaveChangesAsync(ct);
        // Reload with includes for DTO mapping (roles/accesses)
        var withIncludes = await _users.GetWithAccessesAsync(entity.Id, ct)
                           ?? throw new InvalidOperationException("Failed to reload user with accesses.");

        return withIncludes.ToDto();
    }
}
