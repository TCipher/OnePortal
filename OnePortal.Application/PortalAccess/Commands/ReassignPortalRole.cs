using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.PortalAccess.Dtos;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.PortalAccess.Commands;

public record ReassignPortalRoleCommand(int UserId, int PortalId, int NewPortalRoleId) : IRequest<UserPortalAccessItemDto>;

public class ReassignPortalRoleValidator : AbstractValidator<ReassignPortalRoleCommand>
{
    public ReassignPortalRoleValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PortalId).GreaterThan(0);
        RuleFor(x => x.NewPortalRoleId).GreaterThan(0);
    }
}

public class ReassignPortalRoleHandler : IRequestHandler<ReassignPortalRoleCommand, UserPortalAccessItemDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _current;
    private readonly IPermissionService _perm;

    public ReassignPortalRoleHandler(
        IUnitOfWork uow,
        ICurrentUser current,
        IPermissionService perm)
    {
        _uow = uow;
        _current = current;
        _perm = perm;
    }

    public async Task<UserPortalAccessItemDto> Handle(ReassignPortalRoleCommand r, CancellationToken ct)
    {
        if (!await _perm.CanManagePortalAsync(r.PortalId, ct))
            throw new UnauthorizedAccessException("Requires SuperAdmin or PortalAdmin for this portal.");
        if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin))
            throw new UnauthorizedAccessException("SuperAdmin role required.");

        var access = await _uow.UserPortalAccesses.GetWithJoinsByUserAndPortalAsync(r.UserId, r.PortalId, ct)
                     ?? throw new KeyNotFoundException("Portal access not found.");

        var newRole = await _uow.PortalRoles.GetByIdAsync(r.NewPortalRoleId, ct)
                       ?? throw new KeyNotFoundException("New portal role not found.");

        if (newRole.PortalId != access.PortalId)
            throw new InvalidOperationException("NewPortalRole must belong to the same Portal.");

        access.PortalRoleId = newRole.Id;
        await _uow.UserPortalAccesses.UpdateAsync(access, ct);
        await _uow.SaveChangesAsync(ct);

        return new UserPortalAccessItemDto
        {
            PortalId = access.PortalId,
            PortalCode = access.Portal.Code,
            PortalName = access.Portal.Name,
            PortalRoleId = access.PortalRoleId,
            PortalRoleCode = newRole.Code,
            PortalRoleName = newRole.Name,
            IsActive = access.IsActive
        };
    }
}

