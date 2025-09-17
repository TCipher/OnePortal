using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.PortalAccess.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.PortalAccess.Commands;

public record AddUserToPortalCommand(int UserId, int PortalId, int PortalRoleId)
    : IRequest<UserPortalAccessItemDto>;

public class AddUserToPortalValidator : AbstractValidator<AddUserToPortalCommand>
{
    public AddUserToPortalValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PortalId).GreaterThan(0);
        RuleFor(x => x.PortalRoleId).GreaterThan(0);
    }
}

public class AddUserToPortalHandler
    : IRequestHandler<AddUserToPortalCommand, UserPortalAccessItemDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _current;
    private readonly IPermissionService _perm;

    public AddUserToPortalHandler(
        IUnitOfWork uow,
        ICurrentUser current,
        IPermissionService perm)
    {
        _uow = uow;
        _current = current;
        _perm = perm;
    }

    public async Task<UserPortalAccessItemDto> Handle(AddUserToPortalCommand r, CancellationToken ct)
    {
        // 1. Authorization
        if (!await _perm.CanManagePortalAsync(r.PortalId, ct))
            throw new UnauthorizedAccessException("Requires SuperAdmin or PortalAdmin for this portal.");

        if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin))
            throw new UnauthorizedAccessException("SuperAdmin role required (temporary until Sprint 3 RBAC).");

        // 2. Lookups via repositories
        var user = await _uow.Users.GetByIdAsync(r.UserId, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        var portal = await _uow.Portals.GetByIdAsync(r.PortalId, ct)
                     ?? throw new KeyNotFoundException("Portal not found.");

        var role = await _uow.PortalRoles.GetByIdAsync(r.PortalRoleId, ct)
                   ?? throw new KeyNotFoundException("Portal role not found.");

        if (role.PortalId != portal.Id)
            throw new InvalidOperationException("PortalRole does not belong to the specified Portal.");

        // 3. Ensure no duplicate access
        var exists = await _uow.UserPortalAccesses.GetByUserAndPortalAsync(user.Id, portal.Id, ct);
        if (exists is not null)
            throw new InvalidOperationException("Access already exists. Use reassign/activate as appropriate.");

        // 4. Create access
        var entity = new UserPortalAccess
        {
            UserId = user.Id,
            PortalId = portal.Id,
            PortalRoleId = role.Id,
            IsActive = true,
            AssignedOn = DateTime.UtcNow
        };

        await _uow.UserPortalAccesses.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        // 5. Reload with joins (Portal + Role)
        var withJoins = await _uow.UserPortalAccesses.GetWithJoinsAsync(entity.Id, ct)
                        ?? throw new InvalidOperationException("Failed to reload with joins.");

        // 6. Map to DTO
        return new UserPortalAccessItemDto
        {
            PortalId = withJoins.PortalId,
            PortalCode = withJoins.Portal.Code,
            PortalName = withJoins.Portal.Name,
            PortalRoleId = withJoins.PortalRoleId,
            PortalRoleCode = withJoins.PortalRole.Code,
            PortalRoleName = withJoins.PortalRole.Name,
            IsActive = withJoins.IsActive
        };
    }
}
