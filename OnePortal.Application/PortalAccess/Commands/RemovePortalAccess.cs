using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.PortalAccess.Commands;

public record RemovePortalAccessCommand(int UserId, int PortalId) : IRequest<bool>;

public class RemovePortalAccessValidator : AbstractValidator<RemovePortalAccessCommand>
{
    public RemovePortalAccessValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.PortalId).GreaterThan(0);
    }
}

public class RemovePortalAccessHandler : IRequestHandler<RemovePortalAccessCommand, bool>
{
    private readonly IUserPortalAccessRepository _access;
    private readonly ICurrentUser _current;
    private readonly IPermissionService _perm;

    public RemovePortalAccessHandler(
        IUserPortalAccessRepository access,
        ICurrentUser current,
        IPermissionService perm)
    {
        _access = access;
        _current = current;
        _perm = perm;
    }

    public async Task<bool> Handle(RemovePortalAccessCommand r, CancellationToken ct)
    {
        if (!await _perm.CanManagePortalAsync(r.PortalId, ct))
            throw new UnauthorizedAccessException("Requires SuperAdmin or PortalAdmin for this portal.");
        if (!_current.IsInGlobalRole(GlobalRoleCodes.SuperAdmin))
            throw new UnauthorizedAccessException("SuperAdmin role required.");

        var access = await _access.GetByUserAndPortalIncludingInactiveAsync(r.UserId, r.PortalId, ct)
                     ?? throw new KeyNotFoundException("Portal access not found.");

        await _access.DeleteAsync(access, ct);
        return true;
    }
}

