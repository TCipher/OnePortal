using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.PortalAccess.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.PortalAccess.Queries;

public record GetPortalUsersQuery(int PortalId) : IRequest<List<PortalUserItemDto>>;

public class GetPortalUsersValidator : AbstractValidator<GetPortalUsersQuery>
{
    public GetPortalUsersValidator() => RuleFor(x => x.PortalId).GreaterThan(0);
}

public class GetPortalUsersHandler : IRequestHandler<GetPortalUsersQuery, List<PortalUserItemDto>>
{
    private readonly IUserPortalAccessRepository _access;

    public GetPortalUsersHandler(IUserPortalAccessRepository access) => _access = access;

    public async Task<List<PortalUserItemDto>> Handle(GetPortalUsersQuery r, CancellationToken ct)
    {
        
        var accesses = await _access.GetUsersByPortalAsync(r.PortalId, ct);

        
        return accesses.Select(a => new PortalUserItemDto
        {
            UserId = a.UserId,
            EmailAddress = a.User.EmailAddress,
            FullName = (a.User.FirstName + " " +
                        (a.User.MiddleName != null ? a.User.MiddleName + " " : "") +
                        a.User.LastName).Trim(),
            PortalRoleId = a.PortalRoleId,
            PortalRoleCode = a.PortalRole.Code,
            PortalRoleName = a.PortalRole.Name,
            IsActive = a.IsActive
        }).ToList();
    }
}
