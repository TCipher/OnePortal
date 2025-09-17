using FluentValidation;
using MediatR;
using OnePortal.Application.Users.Dtos;
using OnePortal.Application.Common;
using OnePortal.Application.Abstractions;

namespace OnePortal.Application.Users.Queries;

public record GetUserByIdQuery(int Id) : IRequest<UserDto>;

public class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUnitOfWork _uow;

    public GetUserByIdHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<UserDto> Handle(GetUserByIdQuery req, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithAccessesAsync(req.Id, ct);
        if (user is null) throw new KeyNotFoundException("User not found.");

        return user.ToDto();
    }
}
