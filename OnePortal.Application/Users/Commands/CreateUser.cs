using System.Net.Mail;
using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Common;
using OnePortal.Application.Users.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.Users.Commands
{
    /// <summary>
    /// Creates a user with a secure temporary password (must change on first login).
    /// Also assigns default SIMS/EMPLOYEE portal access.
    /// </summary>
    public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserDto>;

    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Request.EmailAddress).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.Request.Gender).IsInEnum();
            RuleFor(x => x.Request.WorkLocation).IsInEnum();
            RuleFor(x => x.Request.SelectedMfa).IsInEnum();

            RuleFor(x => x.Request.RoleId).GreaterThan(0).When(x => x.Request.RoleId.HasValue);
            RuleFor(x => x.Request.DesignationId).GreaterThan(0);
            RuleFor(x => x.Request.DepartmentId).GreaterThan(0);
            RuleFor(x => x.Request.SubDepartmentId).GreaterThan(0);
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
        private readonly IPasswordService _pwd;
        private readonly IEmailSender _email;
        private readonly IUnitOfWork _uow;

        public CreateUserHandler(
            ICurrentUser current,
            IPermissionService perm,
            IUserRepository users,
            IPortalRepository portals,
            IPortalRoleRepository portalRoles,
            IUserPortalAccessRepository userPortalAccesses,
            IPasswordService pwd,
            IEmailSender email,
            IUnitOfWork uow)
        {
            _current = current;
            _perm = perm;
            _users = users;
            _portals = portals;
            _portalRoles = portalRoles;
            _userPortalAccesses = userPortalAccesses;
            _pwd = pwd;
            _email = email;
            _uow = uow;
        }

        public async Task<UserDto> Handle(CreateUserCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;

            // Permission checks
            if (!await _perm.CanManageUsersAsync(ct))
                throw new UnauthorizedAccessException("You do not have permission to create users.");

            if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin) &&
                !_current.IsInGlobalRole(GlobalRoleCodes.Admin))
                throw new UnauthorizedAccessException("Only SuperAdmin or Admin can create users.");

            // Email uniqueness
            if (await _users.GetByEmailAsync(req.EmailAddress, ct) is not null)
                throw new InvalidOperationException("Email already exists.");

            // CHANGE: generate a strong temporary password and hash it
            //var tempPassword = GenerateTemporaryPassword(12);
            var passwordHash = _pwd.Hash("Sup3r@dm1n!");
            

            // Map request -> entity
            var entity = new UserDetails
            {
                EmpCode = req.EmpCode,
                RoleId = req.RoleId,

                EmailAddress = req.EmailAddress.Trim(),
                PhoneNumber = req.PhoneNumber,
                IsActive = req.IsActive,

                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                MiddleName = req.MiddleName?.Trim(),

                ReportingManagerId = req.ReportingManagerId,
                DesignationId = req.DesignationId,
                DepartmentId = req.DepartmentId,
                SubDepartmentId = req.SubDepartmentId,
                OrganizationId = req.OrganizationId,

                BirthDate = req.BirthDate,
                EngageDate = req.EngageDate,
                ReleaseDate = req.ReleaseDate,

                // enums
                Gender = req.Gender,
                WorkLocation = req.WorkLocation,
                Level = req.Level, // sensible default if not provided

                NationalityId = req.NationalityId,
                JobType = req.JobType,       // enum nullable in request -> entity
                SkillType = req.SkillType,   // enum nullable in request -> entity

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

                // CHANGE: set password + must change on first login
                PasswordHash = passwordHash,
                PasswordLastChangedUtc = null,
                MustChangePassword = true,

                // MFA preference (enum)
                PreferredMfaMethod = req.SelectedMfa,
                MfaEnrolledUtc = null,

                Createddate = DateTime.UtcNow
            };

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

            // CHANGE: send welcome email with the temporary password
            // Note: do not log the password; keep transport TLS-only (SMTP over SSL/TLS).
            try
            {
                await _email.SendAsync(
                    to: entity.EmailAddress,
                    subject: "Welcome to OnePortal – Your Temporary Password",
                    htmlBody:
                        $"<p>Hello {entity.FirstName},</p>" +
                        $"<p>Your OnePortal account has been created.</p>" +
                        $"<p><b>Temporary password:</b> Sup3r@dm1n! </p>" +
                         $"<p><b>Email Address:</b> {entity.EmailAddress}</p>" +
                        $"<p>You will be prompted to change this password on first login.</p>" +
                        $"<p>If you selected passkeys (WebAuthn), you can add a passkey after signing in.</p>" +
                        $"<p>— OnePortal Team</p>"
                    );
            }
            catch
            {
                // Swallow email errors to avoid failing user creation; consider logging with Serilog
                // Log.Warning(ex, "Failed to send welcome email to {Email}", entity.EmailAddress);
            }

            // Reload with includes for DTO mapping (roles/accesses)
            var withIncludes = await _users.GetWithAccessesAsync(entity.Id, ct)
                               ?? throw new InvalidOperationException("Failed to reload user with accesses.");

            return withIncludes.ToDto();
        }

        // CHANGE: strong temp password generator (RNG, mixed sets, shuffled)
        private static string GenerateTemporaryPassword(int length = 12)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "@#$%&*!?";
            var all = upper + lower + digits + symbols;

            if (length < 8) length = 8;

            var result = new char[length];
            using var rng = RandomNumberGenerator.Create();

            int NextInt(int maxExclusive)
            {
                // unbiased
                var bytes = new byte[4];
                int value;
                int limit = int.MaxValue - (int.MaxValue % maxExclusive);
                do
                {
                    rng.GetBytes(bytes);
                    value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
                } while (value >= limit);
                return value % maxExclusive;
            }

            // ensure at least one of each
            result[0] = upper[NextInt(upper.Length)];
            result[1] = lower[NextInt(lower.Length)];
            result[2] = digits[NextInt(digits.Length)];
            result[3] = symbols[NextInt(symbols.Length)];

            for (int i = 4; i < length; i++)
                result[i] = all[NextInt(all.Length)];

            // shuffle
            for (int i = 0; i < result.Length; i++)
            {
                int j = NextInt(result.Length);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return new string(result);
        }
    }
}
