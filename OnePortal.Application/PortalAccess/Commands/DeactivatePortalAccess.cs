using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.PortalAccess.Commands;

public record DeactivatePortalAccessCommand(int UserId, int PortalId) : IRequest<bool>;

public class DeactivatePortalAccessValidator : AbstractValidator<DeactivatePortalAccessCommand>
{
    public DeactivatePortalAccessValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PortalId).GreaterThan(0);
    }
}

public class DeactivatePortalAccessHandler : IRequestHandler<DeactivatePortalAccessCommand, bool>
{
    private readonly IUserPortalAccessRepository _access;
    private readonly ICurrentUser _current;
    private readonly IPermissionService _perm;

    public DeactivatePortalAccessHandler(
        IUserPortalAccessRepository access,
        ICurrentUser current,
        IPermissionService perm)
    {
        _access = access;
        _current = current;
        _perm = perm;
    }

    public async Task<bool> Handle(DeactivatePortalAccessCommand r, CancellationToken ct)
    {
        if (!await _perm.CanManagePortalAsync(r.PortalId, ct))
            throw new UnauthorizedAccessException("Requires SuperAdmin or PortalAdmin for this portal.");
        if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin))
            throw new UnauthorizedAccessException("SuperAdmin role required.");

        var access = await _access.GetByUserAndPortalAsync(r.UserId, r.PortalId, ct)
                     ?? throw new KeyNotFoundException("Active portal access not found.");

        access.IsActive = false;
        access.RevokedOn = DateTime.UtcNow;
        await _access.UpdateAsync(access, ct);

        return true;
    }
}
