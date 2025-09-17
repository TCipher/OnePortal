using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Common;
using OnePortal.Application.Users.Dtos;

namespace OnePortal.Application.Users.Queries;

public record GetUsersPagedQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserListItemDto>>;

public class GetUsersPagedValidator : AbstractValidator<GetUsersPagedQuery>
{
    public GetUsersPagedValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}

public class GetUsersPagedHandler : IRequestHandler<GetUsersPagedQuery, PagedResult<UserListItemDto>>
{
    private readonly IUnitOfWork _uow;
    public GetUsersPagedHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<UserListItemDto>> Handle(GetUsersPagedQuery req, CancellationToken ct)
    {
        return await _uow.Users.GetPagedAsync(req.Page, req.PageSize, ct);
    }
}
