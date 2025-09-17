using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.PortalAccess.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.PortalAccess.Queries;

public record GetUserPortalAccessesQuery(int UserId) : IRequest<List<UserPortalAccessItemDto>>;

public class GetUserPortalAccessesValidator : AbstractValidator<GetUserPortalAccessesQuery>
{
    public GetUserPortalAccessesValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}

public class GetUserPortalAccessesHandler
    : IRequestHandler<GetUserPortalAccessesQuery, List<UserPortalAccessItemDto>>
{
    private readonly IUserPortalAccessRepository _access;

    public GetUserPortalAccessesHandler(IUserPortalAccessRepository access) => _access = access;

    public async Task<List<UserPortalAccessItemDto>> Handle(GetUserPortalAccessesQuery r, CancellationToken ct)
    {
        var list = await _access.GetByUserWithJoinsAsync(r.UserId, ct);
        return list.Select(a => new UserPortalAccessItemDto
        {
            PortalId = a.PortalId,
            PortalCode = a.Portal.Code,
            PortalName = a.Portal.Name,
            PortalRoleId = a.PortalRoleId,
            PortalRoleCode = a.PortalRole.Code,
            PortalRoleName = a.PortalRole.Name,
            IsActive = a.IsActive
        }).ToList();
    }
}

