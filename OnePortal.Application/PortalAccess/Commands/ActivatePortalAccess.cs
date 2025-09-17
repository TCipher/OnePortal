using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.PortalAccess.Commands;

public record ActivatePortalAccessCommand(int UserId, int PortalId) : IRequest<bool>;

public class ActivatePortalAccessValidator : AbstractValidator<ActivatePortalAccessCommand>
{
    public ActivatePortalAccessValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PortalId).GreaterThan(0);
    }
}

public class ActivatePortalAccessHandler : IRequestHandler<ActivatePortalAccessCommand, bool>
{
    private readonly IUserPortalAccessRepository _access;
    private readonly ICurrentUser _current;
    private readonly IPermissionService _perm;

    public ActivatePortalAccessHandler(IUserPortalAccessRepository access, ICurrentUser current, IPermissionService perm)
    {
        _access = access;
        _current = current;
        _perm = perm;
    }

    public async Task<bool> Handle(ActivatePortalAccessCommand r, CancellationToken ct)
    {
        if (!await _perm.CanManagePortalAsync(r.PortalId, ct))
            throw new UnauthorizedAccessException("Requires SuperAdmin or PortalAdmin for this portal.");
        if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin))
            throw new UnauthorizedAccessException("SuperAdmin role required (temporary until Sprint 3 RBAC).");

        var access = await _access.GetByUserAndPortalAsync(r.UserId, r.PortalId, ct)
                     ?? throw new KeyNotFoundException("Portal access not found.");

        access.IsActive = true;
        access.RevokedOn = null;
        await _access.UpdateAsync(access, ct);

        return true;
    }
}
