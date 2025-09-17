using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Common;
using OnePortal.Application.Users.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.Users.Commands;

public record UpdateUserCommand(
    int Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    int? RoleId
) : IRequest<UserDto>;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateUserHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<UserDto> Handle(UpdateUserCommand req, CancellationToken ct)
    {
        var user = await _uow.Users.GetWithAccessesAsync(req.Id, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        user.FirstName = req.FirstName.Trim();
        user.LastName = req.LastName.Trim();
        user.MiddleName = req.MiddleName?.Trim();
        user.PhoneNumber = req.PhoneNumber;
        user.RoleId = req.RoleId;
        user.Modifieddate = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return user.ToDto();
    }
}
